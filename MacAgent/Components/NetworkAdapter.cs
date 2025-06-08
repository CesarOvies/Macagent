using System;
using System.Collections.Generic;
using System.Net;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-networkadapter

namespace MacAgent.Components;

public class NetworkAdapter
{
    public string AdapterType { get; set; } = string.Empty;

    public string Caption { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string MACAddress { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string NetConnectionID { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public UInt64 Speed { get; set; }

    public UInt64 BytesSentPersec { get; set; }

    public UInt64 BytesReceivedPersec { get; set; }

    // https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-networkadapterconfiguration

    public List<IPAddress> DefaultIPGatewayList { get; set; } = new List<IPAddress>();

    public IPAddress DHCPServer { get; set; } = IPAddress.None;

    public List<IPAddress> DNSServerSearchOrderList { get; set; } = new List<IPAddress>();

    public List<IPAddress> IPAddressList { get; set; } = new List<IPAddress>();

    public List<IPAddress> IPSubnetList { get; set; } = new List<IPAddress>();

}