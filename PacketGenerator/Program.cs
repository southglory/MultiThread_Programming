using System.Xml;

namespace PacketGenerator
{
    internal class Program
    {
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
            }
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)//Endelement인데 잘못들어왔다면 그냥 리턴. 
                return;

            if (r.Name.ToLower() != "packet")//패킷이라는 이름이 아니면 리턴.
                Console.WriteLine("Invalid packet node");
                return;

            string packetName = r["name"];//노드의 이름은...
            if (string.IsNullOrEmpty(packetName))//만일 이름이 없다면 그냥 리턴.
            {
                Console.WriteLine("Packet without name");
                return;
            }
            //이제 패킷이라는 이름의 노드가 분명하다.
            ParseMembers(r);//그 멤버를 파싱한다.
        }

        public static void ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            int depth = r.Depth + 1;//파싱하려는 정보의 depth.<packet>보다 깊으므로 +1.
            while (r.Read())
            {
                if (r.Depth != depth)//파싱이 끝나는 </packet>지점의 depth. 
                    break;

                string memberName = r["name"];//멤버 이름.
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }

                string memberType = r.Name.ToLower();//멤버 타입.
                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list": // list는 우리가 정의한 타입.
                        break;
                    default:
                        break;
                }
            }
        }

    }
}