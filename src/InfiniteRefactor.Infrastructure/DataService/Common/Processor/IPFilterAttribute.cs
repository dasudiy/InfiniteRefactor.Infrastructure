using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Abstractions.Processor;
using System;
using System.Net;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    public class IPFilterAttribute : PreProcessorAttribute
    {
        public virtual string[] IPList { get; set; }
        public IPFilterModeEnum IPFilterMode { get; set; }

        public IPFilterAttribute()
        {
            this.Priority = 5;
        }

        public enum IPFilterModeEnum
        {
            BlackListMode,
            WhiteListMode
        }

        public override ProcessResult Process(DataServiceRequest request)
        {
            var requestIP = request.Remote.Address.MapToIPv4();
            var canAccess = CheckIP(requestIP, IPFilterMode, IPList);

            var result = new ProcessResult();
            result.CancelProcess = !canAccess;
            result.SourceName = this.GetType().Name;
            if (!canAccess)
            {
                result.Message = string.Format("Access Denied({0}).", request.Remote.Address.ToString());
                result.Last = true;
            };
            return result;
        }

        public static bool CheckIP(IPAddress requestIP, IPFilterModeEnum mode, string[] ipList)
        {
            bool canAccess = false;
            if (mode == IPFilterModeEnum.WhiteListMode)
            {
                foreach (var item in ipList)
                {
                    var parts = item.Split('/');
                    var ip = IPAddress.Parse(parts[0]);

                    var cidr = parts.Length == 1 ? 32 : int.Parse(parts[1]);
                    var mask = new byte[4];
                    for (var i = 0; i < 4; i++)
                    {
                        if (cidr > 8)
                        {
                            mask[i] = 0xff;
                        }
                        else if (cidr < 0)
                        {
                            mask[i] = 0;
                        }
                        else
                        {
                            mask[i] = Convert.ToByte(Convert.ToInt32(string.Empty.PadLeft(cidr, '1').PadRight(8, '0'), 2));
                        }
                        cidr -= 8;
                    }

                    canAccess |= IPAddressExtensions.IsInSameSubnet(requestIP, ip, new IPAddress(mask));
                }
            }
            else
            {
                canAccess = true;
                foreach (var item in ipList)
                {
                    var parts = item.Split('/');
                    var ip = IPAddress.Parse(parts[0]);
                    var mask = new IPAddress(0xFFFFFFFF << (32 - (parts.Length > 1 ? int.Parse(parts[1]) : 32)));

                    canAccess &= !IPAddressExtensions.IsInSameSubnet(requestIP, ip, mask);
                }
            }
            return canAccess;
        }
    }

    public static class IPAddressExtensions
    {
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }
    }
}