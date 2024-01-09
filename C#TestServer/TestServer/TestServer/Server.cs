using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Transactions;


var tcpListener = new TcpListener(IPAddress.Any, 2024);
Zps[][] Tbl = [new Zps[5], new Zps[5]];

try
{
    tcpListener.Start();    // запускаем сервер
    Console.WriteLine("Сервер запущен. Ожидание подключений... ");

    while (true)
    {
        // получаем подключение в виде TcpClient
        using var tcpClient = await tcpListener.AcceptTcpClientAsync();
        // получаем объект NetworkStream для взаимодействия с клиентом
        var stream = tcpClient.GetStream();
        // буфер для входящих данных
        var response = new List<byte>();
        int bytesRead = 10;
        // считываем данные до конечного символа
        while ((bytesRead = stream.ReadByte()) != '\n')
        {
            // добавляем в буфер
            response.Add((byte)bytesRead);
        }
        var word = Encoding.UTF8.GetString(response.ToArray());

        // если прислан маркер окончания взаимодействия,
        // выходим из цикла и завершаем взаимодействие с клиентом
        switch (word)
        {
            case "GET":
                string str = "";
                foreach (Zps[] row in Tbl)
                {
                    foreach (Zps zps in row)
                    {
                        str += zps.ToString() + ';';
                    }
                    str += '/';
                }
                await stream.WriteAsync(Encoding.UTF8.GetBytes(str + '\n'));
                break;
            default:
                string[] val = word.Split(' ');
                int lvl = int.Parse(val[0]);
                TimeSpan time = TimeSpan.Parse(val[2]);
                int i;

                if (time > Tbl[lvl][4].Time)
                {
                    for (i = 4; i > 0; i--)
                    {
                        Tbl[lvl][i] = Tbl[lvl][i - 1];
                        if (time < Tbl[lvl][i - 1].Time)
                            break;
                    };
                    Tbl[lvl][i].Name = val[1];
                    Tbl[lvl][i].Time = time;
                }

                foreach (Zps[] row in Tbl)
                {
                    foreach (Zps zps in row)
                    {
                        Console.Write(zps.ToString() + '\n');
                    }
                    Console.WriteLine();
                }
                break;
        }
        tcpClient.Close();
        response.Clear();
    }
}
finally
{
    tcpListener.Stop();
}

public struct Zps
{
    public string Name;
    public TimeSpan Time;

    public override string ToString()
    {
        return $"{Name} {Time}";
    }
}
