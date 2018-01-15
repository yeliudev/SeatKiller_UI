﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeatKiller_UI
{
    public static class main
    {
        public static string buildingId, roomId, seatId, date, startTime, endTime;
        public static string[] rooms;
        private static Thread thread;

        public static void Start()
        {
            thread = new Thread(run);
            thread.IsBackground = true;
            thread.Start();
        }

        public static void Stop()
        {
            try
            {
                thread.Abort();
                Config.config.textBox2.AppendText("\r\n\r\n------------------------------抢座模式中断------------------------------\r\n");
            }
            catch
            {
                Config.config.textBox2.AppendText("\r\n\r\n------------------------------抢座模式中断------------------------------\r\n");
            }
        }

        public static void run()
        {
            if (Config.config.comboBox3.SelectedIndex == 1)
            {
                Config.config.textBox2.AppendText("\r\n\r\n------------------------------进入抢座模式------------------------------\r\n");
                SeatKiller.Wait("22", "14", "40");
                bool try_booking = true;
                if (SeatKiller.GetToken() == "success")
                {
                    SeatKiller.GetBuildings();
                    SeatKiller.GetRooms(buildingId);

                    SeatKiller.Wait("22", "15", "00");
                    while (try_booking)
                    {
                        if (seatId != "0")
                        {
                            if (SeatKiller.BookSeat(seatId, date, startTime, endTime) == "Success")
                                break;
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n指定座位预约失败，尝试检索其他空位.....");
                                seatId = "0";
                                continue;
                            }
                        }
                        else if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:45:00")) < 0)
                        {
                            SeatKiller.freeSeats.Clear();
                            if (roomId == "0")
                            {
                                foreach (var room in rooms)
                                {
                                    if (SeatKiller.SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                                    {
                                        Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                        Thread.Sleep(30000);
                                    }
                                }
                            }
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n尝试检索同区域其他座位.....\r\n");
                                if (SeatKiller.SearchFreeSeat(buildingId, roomId, date, startTime, endTime) != "Success")
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n当前区域暂无空位，尝试全馆检索空位.....\r\n");
                                    foreach (var room in rooms)
                                    {
                                        if (SeatKiller.SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                            Thread.Sleep(30000);
                                        }
                                    }
                                }
                            }

                            if (SeatKiller.freeSeats.Count == 0)
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n当前全馆暂无空位，5秒后尝试继续检索空位\r\n");
                                Thread.Sleep(5000);
                            }

                            foreach (var freeSeat in SeatKiller.freeSeats)
                            {
                                string response = SeatKiller.BookSeat(freeSeat.ToString(), date, startTime, endTime);
                                switch (response)
                                {
                                    case "Success":
                                        try_booking = false;
                                        return;
                                    case "Failed":
                                        Thread.Sleep(2000);
                                        break;
                                    case "Connection lost":
                                        DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:45:00");
                                        TimeSpan delta = time.Subtract(DateTime.Now);
                                        Config.config.textBox2.AppendText("\r\n\r\n连接丢失，1分钟后重新尝试抢座，系统开放时间剩余" + delta.TotalSeconds.ToString() + "秒\r\n");
                                        Thread.Sleep(60000);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n抢座失败，座位预约系统已关闭\r\n");
                            break;
                        }
                    }
                }
            }
        }
    }
}