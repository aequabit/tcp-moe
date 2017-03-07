/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

﻿using System;
using System.Text;
using System.Management;

namespace tcp_moe_client.Classes
{
    class Local
    {
        private static string getProperty(string component, string property)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", String.Format("SELECT {0} FROM {1}", property, component));
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    return queryObj[property].ToString();
                }
            }
            catch
            {
            }
            return "";
        }

        public static string CpuManufacturer()
        {
            return getProperty("Win32_Processor", "Manufacturer");
        }

        public static string CpuName()
        {
            return getProperty("Win32_Processor", "Name");
        }

        public static string CpuId()
        {
            return getProperty("Win32_Processor", "ProcessorId");
        }

        public static int CpuCores()
        {
            return int.Parse(getProperty("Win32_Processor", "NumberOfCores"));
        }

        public static string GpuName()
        {
            return getProperty("Win32_VideoController", "Name");
        }

        public static string BiosVersion()
        {
            return getProperty("Win32_BIOS", "Version");
        }

        public static string BiosVendor()
        {
            return getProperty("Win32_BIOS", "Manufacturer");
        }

        public static string MainboardManufacturer()
        {
            return getProperty("Win32_BaseBoard", "Manufacturer");
        }

        public static string MainboardName()
        {
            return getProperty("Win32_BaseBoard", "Product");
        }

        public static string HddName()
        {
            return getProperty("Win32_DiskDrive", "Model");
        }

        public static string HddSerial()
        {
            return getProperty("Win32_DiskDrive", "SerialNumber");
        }

        public static string HWID
        {
            get
            {
                return Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(Profile())
                );
            }
        }

        public static string Profile()
        {
            return String.Format(
                "CPU Manufacturer: {0}\nCPU Name: {1}\nCPU ID: {2}\nCPU Cores: {3}\nGPU Name: {4}\nBIOS Version: {5}\nBIOS Vendor: {6}\nMainboard Manufacturer: {7}\nMainboard Name: {8}\nHDD Name: {9}\nHDD Serial: {10}",
                CpuManufacturer(), CpuName(), CpuId(), CpuCores().ToString(), GpuName(), BiosVersion(), BiosVendor(), MainboardManufacturer(), MainboardName(), HddName(), HddSerial()
                );
        }
    }
}
