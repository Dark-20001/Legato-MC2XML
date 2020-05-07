using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;

namespace MC_XML
{
    public class Program
    {
        public static List<item> items = new List<item>();
        public static Regex regMessageId = new Regex("MessageId=(?<value>.*$)",RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regSeverity = new Regex("Severity=(?<value>.*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regFacility = new Regex("Facility=(?<value>.*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regSymbolicName = new Regex("SymbolicName=(?<value>.*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regLanguage = new Regex("Language=(?<value>.*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regComments = new Regex("^;(?:\\s+)?//.*$", RegexOptions.Compiled);

        public const int preservedHeaderCount = 42;

        public static void Main(string[] args)
        {
#if DEBUG
            args = new string[1] { @"G:\EMC\DEV\MC_XML\TestData\OCTO_MSG.MC.xml" };
            /*
            args = new string[9];
            args[0] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCchs.xml";
            args[1] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCdeu.xml";
            args[2] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCesm.xml";
            args[3] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCfra.xml";
            args[4] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCita.xml";
            args[5] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCjpn.xml";
            args[6] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCkor.xml";
            args[7] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCptb.xml";
            args[8] = @"G:\EMC\Geodrive 1.2.0_AS_Update1\Pre\Convert\resource.1.2.0.19\OCTO_MSG_update.MCrus.xml";
            */
#endif
            for (int i = 0; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    if (Process(args[i]))
                    {
                        Console.WriteLine(string.Format("Process {0} success!", Path.GetFileName(args[0])));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Process {0} failed!", Path.GetFileName(args[0])));
                    }
                }
                else
                {
                    Console.WriteLine("Invalid arguments or File not exists!");
                }
            }
            Console.ReadKey();
        }

        public static bool Process(string file)
        {
            if (Path.GetExtension(file).ToUpper() == ".MC")
            {
                Console.WriteLine("Running MC to XML...");
                return MC2XML(file);
            }
            else if (Path.GetExtension(file).ToUpper() == ".XML") 
            {
                Console.WriteLine("Running XML to MC");
                return XML2MC(file, LanguageNames.English);
            }
            else
            {
                Console.WriteLine("Not supported file type!");
                return false;
            }
        }

        public static bool MC2XML(string file)
        {
            StreamReader sr = new StreamReader(file, Encoding.Unicode, true);
            string line = string.Empty;
            bool isStart = false;
            bool isFirstLine = false;
            string text = string.Empty;

            Match mMessageId = null;
            Match mSeverity = null;
            Match mFacility = null;
            Match mSymbolicName = null;
            Match mLanguage = null;

            int linecounter = 0;

            while (sr.Peek() >= 0)
            {
                linecounter++;
                line = sr.ReadLine();

                if (linecounter > preservedHeaderCount)
                {                    
                    if (regMessageId.IsMatch(line) && isStart == false)
                    {
                        Console.Write(".");

                        isStart = true;
                        item i = new item();
                        i.isComments = false;

                        mMessageId = regMessageId.Match(line);
                        i.MessageId = mMessageId.Groups["value"].Value;
                        line = sr.ReadLine();

                        mSeverity = regSeverity.Match(line);
                        i.Severity = mSeverity.Groups["value"].Value;
                        line = sr.ReadLine();

                        if (regFacility.IsMatch(line))
                        {

                            mFacility = regFacility.Match(line);
                            i.Facility = mFacility.Groups["value"].Value;
                            line = sr.ReadLine();

                            mSymbolicName = regSymbolicName.Match(line);
                            i.SymbolicName = mSymbolicName.Groups["value"].Value;
                            line = sr.ReadLine();

                            while (line == "")
                            {
                                line = sr.ReadLine();
                            }

                            mLanguage = regLanguage.Match(line);
                            i.Language = (LanguageNames)Enum.Parse(typeof(LanguageNames), mLanguage.Groups["value"].Value);
                            isFirstLine = true;

                            line = sr.ReadLine();
                            while (line != ".")
                            {
                                if (isFirstLine)
                                {
                                    text = line;
                                    isFirstLine = false;
                                }
                                else
                                {
                                    text = text + "\r\n" + line;
                                }
                                line = sr.ReadLine();
                            }
                            isFirstLine = false;
                            isStart = false;
                            i.Text = text;

                            items.Add(i);
                            text = string.Empty;
                        }
                        else if (regSymbolicName.IsMatch(line))
                        {
                            mSymbolicName = regSymbolicName.Match(line);
                            i.SymbolicName = mSymbolicName.Groups["value"].Value;
                            line = sr.ReadLine();

                            while (line == "")
                            {
                                line = sr.ReadLine();
                            }

                            mLanguage = regLanguage.Match(line);
                            i.Language = (LanguageNames)Enum.Parse(typeof(LanguageNames), mLanguage.Groups["value"].Value);
                            isFirstLine = true;

                            line = sr.ReadLine();
                            while (line != ".")
                            {
                                if (isFirstLine)
                                {
                                    text = line;
                                    isFirstLine = false;
                                }
                                else
                                {
                                    text = text + "\r\n" + line;
                                }
                                line = sr.ReadLine();
                            }
                            isFirstLine = false;
                            isStart = false;
                            i.Text = text;

                            items.Add(i);
                            text = string.Empty;
                        }
                        else
                        {
                            //should not be here
                            isStart = false;
                            isFirstLine = false;
                            Console.WriteLine("Error! Missing SymbolicName!");
                            return false;
                        }
                    }
                    else if (regComments.IsMatch(line))
                    {
                        item i = new item();
                        i.isComments = true;
                        i.Text = line;
                        items.Add(i);
                    }
                }


            }

            sr.Close();

            StreamWriter sw = new StreamWriter(file + ".XML", false, Encoding.Unicode);
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "Unicode", string.Empty);
            doc.AppendChild(dec);

            XmlElement root = doc.CreateElement("resources");

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].isComments)
                {
                    XmlElement elem_item = doc.CreateElement("item");

                    XmlAttribute AttComments = doc.CreateAttribute("isComments");
                    AttComments.Value = "yes";
                    elem_item.Attributes.Append(AttComments);

                    XmlElement elem_Text = doc.CreateElement("Comment");
                    XmlCDataSection cdata = doc.CreateCDataSection(items[i].Text);
                    elem_Text.AppendChild(cdata);

                    elem_item.AppendChild(elem_Text);
                    root.AppendChild(elem_item);
                }
                else
                {
                    XmlElement elem_item = doc.CreateElement("item");

                    XmlAttribute AttComments = doc.CreateAttribute("isComments");
                    AttComments.Value = "no";
                    elem_item.Attributes.Append(AttComments);

                    XmlElement elem_MessageId = doc.CreateElement("MessageId");
                    elem_MessageId.InnerText = items[i].MessageId;

                    XmlElement elem_Severity = doc.CreateElement("Severity");
                    elem_Severity.InnerText = items[i].Severity;

                    if (items[i].Facility != null)
                    {
                        XmlElement elem_Facility = doc.CreateElement("Facility");
                        elem_Facility.InnerText = items[i].Facility;
                        elem_item.AppendChild(elem_Facility);
                    }

                    //XmlElement elem_SymbolicName = doc.CreateElement("SymbolicName");
                    //elem_SymbolicName.InnerText = items[i].SymbolicName;
                    XmlElement elem_Language = doc.CreateElement("Language");
                    elem_Language.InnerText = Enum.GetName(typeof(LanguageNames), items[i].Language);
                    XmlElement elem_Text = doc.CreateElement("Text");
                    XmlCDataSection cdata = doc.CreateCDataSection(items[i].Text);
                    elem_Text.AppendChild(cdata);

                    XmlAttribute attSymbolicName = doc.CreateAttribute("SymbolicName");
                    attSymbolicName.Value = items[i].SymbolicName;
                    elem_Text.Attributes.Append(attSymbolicName);


                    elem_item.AppendChild(elem_MessageId);


                    elem_item.AppendChild(elem_Severity);

                    //elem_item.AppendChild(elem_SymbolicName);
                    elem_item.AppendChild(elem_Language);
                    elem_item.AppendChild(elem_Text);

                    root.AppendChild(elem_item);
                }
            }
            doc.AppendChild(root);
            doc.Save(sw);
            sw.Close();
            
            return true;
        }

        public static bool XML2MC(string file, LanguageNames language)
        {
            StreamReader sr = new StreamReader(file, Encoding.Unicode, true);
            StreamWriter sw = new StreamWriter(file+(".")+Enum.GetName(typeof(LanguageNames), language), false, Encoding.Unicode);

            XmlDocument doc = new XmlDocument();
            doc.Load(sr);

            XmlElement root = doc.DocumentElement;
            XmlNodeList itemsNode = root.SelectNodes("item");

            XmlNode ndMessageId=null;
            XmlNode ndSeverity=null;
            XmlNode ndFacility=null;
            //XmlNode ndSymbolicName=null;
            XmlNode ndLanguage=null;
            XmlNode ndText=null;
            XmlCDataSection cdata=null;

            foreach(XmlNode itemNode in itemsNode)
            {
                if (itemNode.Attributes["isComments"].Value == "no")
                {
                    ndMessageId = itemNode.SelectSingleNode("MessageId");

                    sw.WriteLine(string.Format("MessageId={0}", ndMessageId.InnerText));


                    ndSeverity = itemNode.SelectSingleNode("Severity");
                    sw.WriteLine(string.Format("Severity={0}", ndSeverity.InnerText));

                    ndFacility = itemNode.SelectSingleNode("Facility");
                    if (ndFacility != null)
                    {
                        sw.WriteLine(string.Format("Facility={0}", ndFacility.InnerText));
                    }

                    ndText = itemNode.SelectSingleNode("Text");
                    //ndSymbolicName = itemNode.SelectSingleNode("SymbolicName");
                    //sw.WriteLine(string.Format("SymbolicName={0}", ndSymbolicName.InnerText));
                    sw.WriteLine(string.Format("SymbolicName={0}", ndText.Attributes["SymbolicName"].Value));

                    sw.WriteLine(string.Format("Language={0}", Enum.GetName(typeof(LanguageNames), language)));

                    sw.WriteLine(ndText.InnerText);

                    sw.WriteLine(".");
                    sw.WriteLine("");
                }
                else
                {
                    ndText = itemNode.SelectSingleNode("Comment");
                    sw.WriteLine(ndText.InnerText);
                }
            }

            sr.Close();
            sw.Close();

            return true; 
        }

        public enum LanguageNames 
        {
            English=1033,
            Chinese=2052,
	        French=1036,
	        Korean=1042,
	        Japanese=1041,
	        German=1031,
	        Russian=1049,
	        Spanish=2058,
	        Italian=1040,
	        Portuguese=1046
            //Enum.Parse(typeof(Colour), "Red", true)
            //string c2string=Enum.GetName(typeof(Colour), c);
        }

        public class item
        {
            public string MessageId { get; set; }
            public string Severity { get; set; }
            public string Facility { get; set; } //Optional
            public string SymbolicName { get; set; }
            public LanguageNames Language { get; set; }
            public string Text { get; set; }
            public bool isComments { get; set; }
        }
    }
}
