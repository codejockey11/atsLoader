using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using aviationLib;

namespace atsLoader
{
    class Fixs
    {
        public String id;
        public String fixId;

        public Fixs(String i, String fid)
        {
            id = i;
            fixId = fid;
        }
    }

    class Navaids
    {
        public String id;
        public String navId;

        public Navaids(String i, String nid)
        {
            id = i;
            navId = nid;
        }
    }

    class Program
    {
        static Char[] recordType_001_04 = new Char[04];

        static String MEA_85_05;
        static String MAX_109_05;
        static String MOCA_114_05;

        static StreamWriter ofileATS1 = new StreamWriter("airway2.txt");

        static StreamReader awyFixs = new StreamReader("awyFixs.txt");

        static StreamReader awyNavaids = new StreamReader("awyNavaids.txt");

        static List<Fixs> fixs = new List<Fixs>();

        static List<Navaids> navaids = new List<Navaids>();

        static void Main(String[] args)
        {
            String rec = awyFixs.ReadLine();

            while (!awyFixs.EndOfStream)
            {
                String[] f = rec.Split('~');

                Fixs o = new Fixs(f[0], f[1]);

                fixs.Add(o);

                rec = awyFixs.ReadLine();
            }

            String[] ff = rec.Split('~');

            Fixs oo = new Fixs(ff[0], ff[1]);

            fixs.Add(oo);

            awyFixs.Close();

            rec = awyNavaids.ReadLine();

            while (!awyNavaids.EndOfStream)
            {
                String[] fn = rec.Split('~');

                Navaids on = new Navaids(fn[0], fn[1]);

                navaids.Add(on);

                rec = awyNavaids.ReadLine();
            }

            String[] fnn = rec.Split('~');

            Navaids onn = new Navaids(fnn[0], fnn[1]);

            navaids.Add(onn);

            awyNavaids.Close();

            String userprofileFolder = Environment.GetEnvironmentVariable("USERPROFILE");
            String[] fileEntries = Directory.GetFiles(userprofileFolder + "\\Downloads\\", "28DaySubscription*.zip");

            ZipArchive archive = ZipFile.OpenRead(fileEntries[0]);
            ZipArchiveEntry entry = archive.GetEntry("ATS.txt");
            entry.ExtractToFile("ATS.txt", true);

            StreamReader file = new StreamReader("ATS.txt");

            rec = file.ReadLine();

            while (!file.EndOfStream)
            {
                ProcessRecord(rec);
                rec = file.ReadLine();
            }

            ProcessRecord(rec);

            file.Close();

            ofileATS1.Close();
        }

        static void ProcessRecord(String record)
        {
            recordType_001_04 = record.ToCharArray(0, 4);

            String rt = new String(recordType_001_04);

            Int32 r = String.Compare(rt, "ATS1");
            if (r == 0)
            {
                MEA_85_05 = new String(record.ToCharArray(84, 5)).Trim();
                MAX_109_05 = new String(record.ToCharArray(108, 5)).Trim();
                MOCA_114_05 = new String(record.ToCharArray(113, 5)).Trim();
            }

            r = String.Compare(rt, "ATS2");
            if (r == 0)
            {
                Char[] cr = record.ToCharArray(65, 1);
                if ((cr[0] == 'V') || (cr[0] == 'R') || (cr[0] == 'A') || (cr[0] == 'N'))
                {
                    String route = new String(record.ToCharArray(4, 14));

                    if (String.Compare(route.Substring(0, 2), "AT") == 0)
                    {
                        ofileATS1.Write(route.Substring(2, 12).Trim());
                    }
                    if (String.Compare(route.Substring(0, 2), "BF") == 0)
                    {
                        ofileATS1.Write(route.Substring(2, 12).Trim());
                    }
                    if (String.Compare(route.Substring(0, 2), "PA") == 0)
                    {
                        ofileATS1.Write(route.Substring(2, 12).Trim());
                    }
                    if (String.Compare(route.Substring(0, 2), "PR") == 0)
                    {
                        String r5 = route.Replace("ROUTE ", "RTE");
                        Int32 r5n = Convert.ToInt32(r5.Substring(5, 3));
                        ofileATS1.Write(r5.Substring(2, 3).Trim());
                        ofileATS1.Write(r5n.ToString("D"));
                    }

                    ofileATS1.Write('~');

                    ofileATS1.Write(new String(record.ToCharArray(4, 2)).Trim());
                    ofileATS1.Write('~');

                    Int32 i = Convert.ToInt32(new String(record.ToCharArray(20, 5)).Trim());
                    ofileATS1.Write(i.ToString("D05"));
                    ofileATS1.Write('~');

                    String c = new String(record.ToCharArray(160, 33));
                    String[] s = c.Split('*');

                    if (s[0] == "A")
                    {
                        if (s[1].Length < 5)
                        {
                            ofileATS1.Write(s[1]);
                            ofileATS1.Write('~');
                            ofileATS1.Write(new String(record.ToCharArray(65, 19)).Trim());

                            Navaids nav = LookupNavaid(s[1]);

                            if (nav == null)
                            {
                                ofileATS1.Write('~');
                            }
                            else
                            {
                                ofileATS1.Write('~');
                                ofileATS1.Write(nav.id);
                            }
                        }
                        else
                        {
                            ofileATS1.Write(s[1]);
                            ofileATS1.Write('~');
                            ofileATS1.Write(new String(record.ToCharArray(107, 2)).Trim());

                            Fixs fix = LookupFix(s[1]);

                            if (fix == null)
                            {
                                ofileATS1.Write('~');
                            }
                            else
                            {
                                ofileATS1.Write('~');
                                ofileATS1.Write(fix.id);
                            }
                        }
                    }
                    else if (cr[0] == 'N')
                    {
                        ofileATS1.Write(s[1]);
                        ofileATS1.Write('~');
                        ofileATS1.Write(new String(record.ToCharArray(65, 19)).Trim());

                        Navaids nav = LookupNavaid(s[1]);

                        if (nav == null)
                        {
                            ofileATS1.Write('~');
                        }
                        else
                        {
                            ofileATS1.Write('~');
                            ofileATS1.Write(nav.id);
                        }
                    }
                    else if (cr[0] == 'R')
                    {
                        ofileATS1.Write(s[1]);
                        ofileATS1.Write('~');
                        ofileATS1.Write(new String(record.ToCharArray(107, 2)).Trim());

                        Fixs fix = LookupFix(s[1]);

                        if (fix == null)
                        {
                            ofileATS1.Write('~');
                        }
                        else
                        {
                            ofileATS1.Write('~');
                            ofileATS1.Write(fix.id);
                        }
                    }
                    else
                    {
                        ofileATS1.Write(s[1]);
                        ofileATS1.Write('~');
                        ofileATS1.Write(new String(record.ToCharArray(65, 19)).Trim());

                        Navaids nav = LookupNavaid(s[1]);

                        if (nav == null)
                        {
                            ofileATS1.Write('~');
                        }
                        else
                        {
                            ofileATS1.Write('~');
                            ofileATS1.Write(nav.id);
                        }
                    }

                    ofileATS1.Write('~');

                    ofileATS1.Write(MEA_85_05);
                    ofileATS1.Write('~');

                    ofileATS1.Write(MAX_109_05);
                    ofileATS1.Write('~');

                    ofileATS1.Write(MOCA_114_05);
                    ofileATS1.Write('~');

                    Char[] lat = record.ToCharArray(109, 14);
                    Char[] lon = record.ToCharArray(123, 14);

                    if (lat[0] != ' ')
                    {
                        LatLon ll = new LatLon(new String(lat).Trim(), new String(lon).Trim());
                        ofileATS1.Write(ll.formattedLat);
                        ofileATS1.Write('~');

                        ofileATS1.Write(ll.formattedLon);
                        ofileATS1.Write(ofileATS1.NewLine);
                    }
                }
            }
        }

        public static Fixs LookupFix(String ident)
        {
            foreach (Fixs f in fixs)
            {
                if (f.fixId == ident)
                {
                    return f;
                }
            }

            return null;
        }

        public static Navaids LookupNavaid(String ident)
        {
            foreach (Navaids n in navaids)
            {
                if (n.navId == ident)
                {
                    return n;
                }
            }

            return null;
        }

    }
}
