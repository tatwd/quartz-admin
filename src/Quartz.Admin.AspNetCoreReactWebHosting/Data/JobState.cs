using System;

namespace Quartz.Admin.AspNetCoreReactWebHosting.Data
{
    public static class JobState
    {
        public static string Deleted => "Deleted";
        public static string Disable => "Disable";
        public static string Started => "Started";
        public static string Paused => "Paused";
        public static string Fired => "Fired";
        public static string Completed => "Completed";
        public static string Exception => "Exception";

    }
}