using ZwiftPlayConsoleApp.Utils.BitConverter;

namespace ZwiftPlayConsoleApp.Utils;

public class ByteBuffer
{
    private int _position;

    private readonly List<byte> _buffer = new();
    private readonly EndianBitConverter _bitConverter;

    public ByteBuffer(byte[]? bytes = null, bool bigEndian = true)
    {
        _bitConverter = bigEndian ? EndianBitConverter.Big : EndianBitConverter.Little;
        if (bytes != null)
        {
            _buffer.AddRange(bytes);
        }
    }

    public byte[] ToArray()
    {
        return _buffer.ToArray();
    }

    public int ReadInt32()
    {
        return EndianBitConverter.Big.ToInt32(GetBytes(4), 0);
    }

    public void WriteInt32(int v)
    {
        var bytes = _bitConverter.GetBytes(v);
        _buffer.AddRange(bytes);
    }

    public void WriteByte(byte v)
    {
        _buffer.Add(v);
    }

    public void WriteBytes(byte[] bytes)
    {
        _buffer.AddRange(bytes);
    }

    private byte[] GetBytes(int pos)
    {
        var bytes = new byte[pos];

        for (int x = _position, i = 0; x < _position + pos; x++, i++)
        {
            bytes[i] = _buffer[x];
        }

        _position += pos;
        return bytes;
    }
}