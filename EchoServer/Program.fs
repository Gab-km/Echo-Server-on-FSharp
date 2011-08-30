open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text

[<EntryPoint>]
let main _ =
    let ipAddr = Dns.GetHostEntry("localhost").AddressList.[0]
    let listener = new TcpListener(ipAddr, 8080)
    try
        listener.Start()
        Console.WriteLine("Echo server starts.")
        let rec myLoop (lsn : TcpListener) =
            use tcp = lsn.AcceptTcpClient ()
            Console.WriteLine("Client has connected.")
            use nStream = tcp.GetStream ()
            let recieveData (ns : NetworkStream) =
                use mStream = new MemoryStream ()
                let resBytes : byte array = Array.zeroCreate 256
                let rec doLoop (ns : NetworkStream) (ms : MemoryStream) (ba: byte array) =
                    let resSize = ns.Read (ba, 0, ba.Length)
                    if resSize = 0 then
                        Console.WriteLine("Client has closed.")
                        Console.Read () |> ignore
                    else
                        ms.Write(ba, 0, resSize)
                        if ns.DataAvailable then
                            doLoop ns ms ba
                        else
                            let enc = Encoding.UTF8
                            Console.WriteLine(enc.GetString(ms.ToArray()))
                doLoop ns mStream resBytes
            recieveData nStream
            myLoop lsn
        myLoop listener
    finally
        listener.Stop ()
        Console.WriteLine("Echo server stops.")
        Console.Read() |> ignore
    0