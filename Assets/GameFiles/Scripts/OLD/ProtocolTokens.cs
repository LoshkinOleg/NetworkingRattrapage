/*
using Bolt;
using UdpKit;

class ConnectionToken : IProtocolToken
{
    public int id;
    public int passwordHash;
    public string answer;

    public void Write(UdpPacket packet)
    {
        packet.WriteInt(id);
        packet.WriteInt(passwordHash);
        packet.WriteString(answer);
    }
    public void Read(UdpPacket packet)
    {
        id = packet.ReadInt();
        passwordHash = packet.ReadInt();
        answer = packet.ReadString();
    }
}
*/