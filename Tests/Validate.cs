using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using Verse;

static class Validate
{
    static int checks;
    static readonly List<string> errors=new();
    public static bool CaptureError(string __0){errors.Add(__0);return false;}
    static void Check(bool ok,string msg){if(!ok)throw new Exception(msg);checks++;}
    static XmlDocument Load(string p){var d=new XmlDocument();d.Load(p);return d;}
    static void Apply(XmlNode n,XmlDocument data)
    {
        var op=(PatchOperation)DirectXmlToObject.GetObjectFromXmlMethod(typeof(PatchOperation))(n,false);
        Check(op.Apply(data),"Patch failure: "+n.OuterXml);
    }
    static void ValidateLabelRepair(string root,string workshop,XmlNode armorRepair)
    {
        const string id="Kiiro_ULWeapon_SniperRifle",bad="kiiro ultra sniper rifle ",good="kiiro ultra sniper rifle";
        var patch=Load(Path.Combine(root,"Patches/KiiroLabelWhitespace.xml"));
        Check(patch.SelectSingleNode("/Patch/Operation/mods/li").InnerText=="Kiiro Equipment Expansion","Label repair scoped to original mod");
        var repair=patch.SelectSingleNode("/Patch/Operation/match");
        var data=Load(Path.Combine(workshop,"3741356350/1.6/Defs/Weapon/Kiiro_UltraRange.xml"));
        var label=data.SelectSingleNode("/Defs/ThingDef[defName='"+id+"']/label");
        Check(label!=null,"Installed upstream sniper definition exists");
        label.InnerText=bad;
        Apply(armorRepair,data);
        Check(label.InnerText==bad,"Existing armor recovery does not cause or change the upstream label typo");
        string before=data.OuterXml;
        Apply(repair,data);
        label=data.SelectSingleNode("/Defs/ThingDef[defName='"+id+"']/label");
        Check(label.InnerText==good,"Known trailing space removed");
        label.InnerText=bad;
        Check(data.OuterXml==before,"All weapon stats, research, descriptions and armor definitions remain unchanged");
        Apply(repair,data);before=data.OuterXml;Apply(repair,data);
        Check(before==data.OuterXml,"Already-fixed label is unchanged on repeated application");
        label=data.SelectSingleNode("/Defs/ThingDef[defName='"+id+"']/label");
        label.InnerText="custom sniper name ";before=data.OuterXml;Apply(repair,data);
        Check(before==data.OuterXml,"Custom/upstream renamed label is preserved even with intentional whitespace");
        var absent=new XmlDocument();absent.LoadXml("<Defs/>");Apply(repair,absent);
        Check(absent.OuterXml=="<Defs />","Missing sniper definition is a safe no-op");
        absent.LoadXml("<Defs><ThingDef><defName>OtherModSniper</defName><label>kiiro ultra sniper rifle </label></ThingDef></Defs>");
        before=absent.OuterXml;Apply(repair,absent);Check(before==absent.OuterXml,"Other mods' matching names are untouched");
    }
    static int Main(string[] args)
    {
        var h=new Harmony("w0bderful.kiiroequipmentrecovery.validation");
        var prefs=AccessTools.Field(typeof(Prefs),"data");var prior=prefs.GetValue(null);
        if(prior==null)prefs.SetValue(null,RuntimeHelpers.GetUninitializedObject(prefs.FieldType));
        h.Patch(AccessTools.Method(typeof(Log),nameof(Log.Error),new[]{typeof(string)}),prefix:new HarmonyMethod(typeof(Validate),nameof(CaptureError)));
        try
        {
            string root=Path.Combine(args[0],"Kiiro-Equipment-Recovery"),workshop=args[2];
            var patch=Load(Path.Combine(root,"Patches/KiiroRestore.xml"));
            Check(patch.SelectSingleNode("/Patch/Operation/mods/li").InnerText=="Kiiro Equipment Expansion","Scope to original mod");
            var seq=patch.SelectSingleNode("/Patch/Operation/match");
            var data=Load(Path.Combine(workshop,"3741356350/1.6/Defs/Apparel/Apparel_ArmorHead.xml"));
            Apply(seq,data);
            string[] ids={"Kiiro_ExoSuit","Kiiro_ShadowExoSuit","Kiiro_PowerArmor","Kiiro_ShadowPowerArmor","Kiiro_HeavyPowerArmor"};
            foreach(string id in ids)Check(data.SelectNodes("/Defs/ThingDef[defName='"+id+"']").Count==1,"Missing armor restored once: "+id);
            Check(data.SelectSingleNode("/Defs/HediffDef[defName='HoldShield_HeavyArmor']")!=null,"Shield hediff restored");
            foreach(string parent in new[]{"Kiiro_ShellBaseV","Kiiro_ShellBaseVI"})Check(data.SelectNodes("/Defs/ThingDef[@Name='"+parent+"']").Count==1,"Abstract parent restored");
            string before=data.OuterXml;Apply(seq,data);Check(data.OuterXml==before,"Repeated repair is a no-op");
            data.SelectSingleNode("/Defs/ThingDef[defName='Kiiro_ExoSuit']/statBases/MaxHitPoints").InnerText="12345";
            Apply(seq,data);Check(data.SelectSingleNode("/Defs/ThingDef[defName='Kiiro_ExoSuit']/statBases/MaxHitPoints").InnerText=="12345","Existing upstream values preserved");
            // A partially repaired upstream installation must retain existing items
            // while the individual missing definitions are restored.
            data.SelectSingleNode("/Defs/ThingDef[defName='Kiiro_HeavyPowerArmor']").ParentNode.RemoveChild(data.SelectSingleNode("/Defs/ThingDef[defName='Kiiro_HeavyPowerArmor']"));
            Apply(seq,data);Check(data.SelectNodes("/Defs/ThingDef[defName='Kiiro_HeavyPowerArmor']").Count==1,"Partial repair fills only missing item");
            var ce=Load(Path.Combine(workshop,"3770623835/1.6/Patches/Kiiro_Armor_CE.xml"));
            foreach(XmlNode op in ce.SelectNodes("/Patch/Operation"))Apply(op,data);
            foreach(var p in new[]{("Kiiro_ExoSuit","36","93","15","10"),("Kiiro_ShadowExoSuit","32","87","15","10"),("Kiiro_PowerArmor","38","105","30","20"),("Kiiro_ShadowPowerArmor","41","98","30","20"),("Kiiro_HeavyPowerArmor","43","110","30","20")})
            {
                var item=data.SelectSingleNode("/Defs/ThingDef[defName='"+p.Item1+"']");
                foreach(var v in new[]{("statBases/ArmorRating_Sharp",p.Item2),("statBases/ArmorRating_Blunt",p.Item3),("equippedStatOffsets/CarryingCapacity",p.Item4),("equippedStatOffsets/CarryWeight",p.Item5),("statBases/StuffEffectMultiplierArmor","13.2")})
                    Check(item.SelectSingleNode(v.Item1)?.InnerText==v.Item2,"Existing CE value: "+p.Item1+" "+v.Item1);
            }
            before=data.OuterXml;Apply(seq,data);Check(before==data.OuterXml,"Repair does not reset CE modifications");
            ValidateLabelRepair(root,workshop,seq);
            Check(errors.Count==0,"No parser or native patch errors: "+string.Join(";",errors));
            Console.WriteLine("PASS: "+checks+" native XML recovery, label correction, preservation and installed CE value checks");return 0;
        }
        catch(Exception e){Console.WriteLine(e);return 1;}
        finally{h.UnpatchAll(h.Id);prefs.SetValue(null,prior);}
    }
}
