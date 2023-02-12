using System.Xml;

namespace PacketGenerator
{
    internal class Program
    {
        static string genPackets;

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", settings))//이 범위에서 벗어나면 알아서 dispose.
            {
                r.MoveToContent();

                while (r.Read())
                {
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element) //Element는 시작부분, EndElement는 끝부분.
                        ParsePacket(r);
                    //Console.WriteLine(r.Name + " " + r["name"]);
                }

                File.WriteAllText("GenPackets.cs", genPackets);//genPackets에다가 추출한 내용을 GenPackets.cs로 만들어줌
            }
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)//Endelement인데 잘못들어왔다면 그냥 리턴. 
                return;

            if (r.Name.ToLower() != "packet")//패킷이라는 이름이 아니면 리턴.
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string packetName = r["name"];//노드의 이름은...
            if (string.IsNullOrEmpty(packetName))//만일 이름이 없다면 그냥 리턴.
            {
                Console.WriteLine("Packet without name");
                return;
            }
            //이제 패킷이라는 이름의 노드가 분명하다.
            Tuple<string, string, string> t = ParseMembers(r);//그 멤버를 파싱한다.
            //패킷 추출
            genPackets += string.Format(PacketFormat.packetFormat,packetName, t.Item1, t.Item2, t.Item3);
        }

        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1;//파싱하려는 정보의 depth.<packet>보다 깊으므로 +1.
            while (r.Read())
            {
                if (r.Depth != depth)//파싱이 끝나는 </packet>지점의 depth. 
                    break;

                string memberName = r["name"];//멤버 이름.
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;//Environment.NewLine은 Enter와 같음
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;//Environment.NewLine은 Enter와 같음
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;//Environment.NewLine은 Enter와 같음

                string memberType = r.Name.ToLower();//멤버 타입.
                switch (memberType)
                {
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list": // list는 우리가 정의한 타입.
                        break;
                    default:
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

    }
}