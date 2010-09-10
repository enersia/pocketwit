using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using System.Threading;
using System.Runtime.InteropServices;
using System.Xml;
using System.Net;

namespace PockeTwit.Position
{
    class RILPositionProvider : PositionProvider
    {
        Thread watcherThread;
        public static RILCELLTOWERINFO rilCellTowerInfo;
        public RILPositionProvider()
        {
            rilCellTowerInfo = new RILCELLTOWERINFO();
        }

        protected override void Enable()
        {
            watcherThread = new Thread(new ThreadStart(RILPositionThread));
            watcherThread.IsBackground = true;
            watcherThread.Start();
        }

        protected override void Disable()
        {
            if (watcherThread != null)
            {
                watcherThread.Abort();
                watcherThread = null;
            }
        }


/*        private GeoCoord FindLocationOfCell(uint cellid, uint mcc, uint mnc, uint lac)
        {
           
            string address = string.Format("http://www.opencellid.org/cell/get?key=myapikey&mnc={0}&mcc={1}&lac={2}&cellid={3}", 
               mnc, mcc, lac, cellid);
            XmlDocument xmlFeed = new RESTApi().GetXML(address);
            if (xmlFeed == null)
                return null;
            XmlNode xmlCell = xmlFeed.SelectSingleNode("/rsp/cell");
            if (xmlCell != null && xmlCell.Attributes["cellId"].Value != "")
            {
                if (Convert.ToInt32(xmlCell.Attributes["cellId"].Value) == cellid)
                {
                    return new GeoCoord(
                            Convert.ToDouble(xmlCell.Attributes["lat"].Value),
                            Convert.ToDouble(xmlCell.Attributes["lon"].Value)
                        );
                }
                else
                {
                    return null;
                }
            }
            return null;
        }*/

        private GeoCoord GoogleFindLocationOfCell(uint cellid, uint mcc, uint mnc, uint lac, uint rxl, uint tadv)
        {
            

            Hashtable jsh = new Hashtable();
            Hashtable cell = new Hashtable();
            ArrayList cells = new ArrayList();

            jsh["version"] = "1.1.0";
            jsh["host"] = "maps.google.com";
            //jsh["cell_towers"] = new ArrayList();
            cell["cell_id"] = cellid;
            cell["location_area_code"] = lac;
            cell["mobile_country_code"] = mcc;
            cell["mobile_network_code"] = mnc;
            cell["signal_strength"] = rxl;
            cell["timing_advance"] = tadv;
            cell["age"] = 0;

            cells.Add(cell);
            jsh["cell_towers"] = cells;

            string json = PockeTwit.JSON.JsonEncode(jsh);

            string address = "http://www.google.com/loc/json";
            /*            string address = string.Format("http://www.opencellid.org/cell/get?key=myapikey&mnc={0}&mcc={1}&lac={2}&cellid={3}",
               mnc, mcc, lac, cellid);*/

            HttpWebResponse response = null;
            try
            {
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

                request.Method = "POST";
                request.SendChunked = false;
                // set the authorisation levels
                request.ContentType = "application/json";
                // set the length of the content
                request.ContentLength = json.Length;
                //ServicePointManager.Expect100Continue = false;
                // set up the stream
                System.IO.Stream reqStream = request.GetRequestStream();
                // write to the stream
                reqStream.Write(System.Text.Encoding.ASCII.GetBytes(json), 0, json.Length);
                // close the stream
                reqStream.Close();

                request.Timeout = 30000; // 30 seconds


                // fetch the response
                response = request.GetResponse() as HttpWebResponse;

                // Read the stream in with a reader..
                // note that .Close() must be called on the stream
                System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());
                string jsonresp = reader.ReadToEnd();
                reader.Close();
                response.Close();
                Hashtable jsonResponse = (Hashtable)((Hashtable)PockeTwit.JSON.JsonDecode(jsonresp))["location"];
                if (jsonResponse != null)
                {
                    return new GeoCoord(
                             Convert.ToDouble(jsonResponse["latitude"]),
                             Convert.ToDouble(jsonResponse["longitude"]),
                             Convert.ToDouble(jsonResponse["accuracy"])
                         );
                }
                else
                {

                    return null;
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    response = (HttpWebResponse)ex.Response;
                    response.Close();
                }
                else
                {
                    //MessageBox.Show(string.Format("Unknown error: {0}", ex.Message));
                }
                return null;
            }
            catch (System.Net.Sockets.SocketException /*sockex*/)
            {
                // probably a read error or some such problem
                // we don't really care
                return null;
            }

        }

        protected void RILPositionThread()
        {
            try
            {
                while (true)
                {
                    GetCellTowerInfo();
                    if (rilCellTowerInfo.dwCellID != 0)
                    {
                        GeoCoord pos = GoogleFindLocationOfCell(rilCellTowerInfo.dwCellID, rilCellTowerInfo.dwMobileCountryCode, rilCellTowerInfo.dwMobileNetworkCode, rilCellTowerInfo.dwLocationAreaCode, rilCellTowerInfo.dwRxLevel, rilCellTowerInfo.dwTimingAdvance);
                        PositionEventArgs pe;
                        if (pos != null)
                            pe = new PositionEventArgs(PositionEventArgs.PositionStatus.Valid, pos, DateTime.UtcNow);
                        else
                            pe = new PositionEventArgs(PositionEventArgs.PositionStatus.Invalid, null, DateTime.UtcNow);
                        OnPositionChanged(pe);
                    }
                    Thread.Sleep(30000);

                }
            }
            catch (ThreadAbortException /*tae*/)
            {
                // thread aborted
            }
        }

        // string used to store the CellID string
        private static string celltowerinfo = "";

        /*
         * Uses RIL to get CellID from the phone.
         */
        public static void GetCellTowerInfo()
        {
            // initialise handles
            IntPtr hRil = IntPtr.Zero;
            IntPtr hRes = IntPtr.Zero;

            // initialise result
            celltowerinfo = "";

            // initialise RIL
            try
            {
                hRes = RIL_Initialize(1,                                        // RIL port 1
                                      new RILRESULTCALLBACK(rilResultCallback), // function to call with result
                                      null,                                     // function to call with notify
                                      0,                                        // classes of notification to enable
                                      0,                                        // RIL parameters
                                      out hRil);                                // RIL handle returned

                if (hRes != IntPtr.Zero)
                {
                    return; //"Failed to initialize RIL";
                }

                // initialised successfully

                // use RIL to get cell tower info with the RIL handle just created
                hRes = RIL_GetCellTowerInfo(hRil);

                // wait for cell tower info to be returned
                waithandle.WaitOne();

                // finished - release the RIL handle
                RIL_Deinitialize(hRil);
            }
            catch(MissingMethodException /*mme*/)
            {
                // RIL DLL not available, just ignore it for now
            }
            // return the result from GetCellTowerInfo
            //return celltowerinfo;
        }


        // event used to notify user function that a response has
        //  been received from RIL
        private static AutoResetEvent waithandle = new AutoResetEvent(false);


        public static void rilResultCallback(uint dwCode,
                                             IntPtr hrCmdID,
                                             IntPtr lpData,
                                             uint cbData,
                                             uint dwParam)
        {
            // create empty structure to store cell tower info in


            // copy result returned from RIL into structure
            Marshal.PtrToStructure(lpData, rilCellTowerInfo);

            // get the bits out of the RIL cell tower response that we want
            celltowerinfo = rilCellTowerInfo.dwCellID + "-" +
                            rilCellTowerInfo.dwLocationAreaCode + "-" +
                            rilCellTowerInfo.dwMobileCountryCode;

            // notify caller function that we have a result
            waithandle.Set();
        }


        // -------------------------------------------------------------------
        //  RIL function definitions
        // -------------------------------------------------------------------

        /* 
         * Function definition converted from the definition 
         *  RILRESULTCALLBACK from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa920069.aspx
         */
        public delegate void RILRESULTCALLBACK(uint dwCode,
                                               IntPtr hrCmdID,
                                               IntPtr lpData,
                                               uint cbData,
                                               uint dwParam);


        /*
         * Function definition converted from the definition 
         *  RILNOTIFYCALLBACK from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa922465.aspx
         */
        public delegate void RILNOTIFYCALLBACK(uint dwCode,
                                               IntPtr lpData,
                                               uint cbData,
                                               uint dwParam);

        /*
         * Class definition converted from the struct definition 
         *  RILCELLTOWERINFO from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa921533.aspx
         */
        public class RILCELLTOWERINFO
        {
            public uint cbSize = 0;
            public uint dwParams = 0;
            public uint dwMobileCountryCode = 0;
            public uint dwMobileNetworkCode = 0;
            public uint dwLocationAreaCode = 0;
            public uint dwCellID = 0;
            public uint dwBaseStationID = 0;
            public uint dwBroadcastControlChannel = 0;
            public uint dwRxLevel = 0;
            public uint dwRxLevelFull = 0;
            public uint dwRxLevelSub = 0;
            public uint dwRxQuality = 0;
            public uint dwRxQualityFull = 0;
            public uint dwRxQualitySub = 0;
            public uint dwIdleTimeSlot = 0;
            public uint dwTimingAdvance = 0;
            public uint dwGPRSCellID = 0;
            public uint dwGPRSBaseStationID = 0;
            public uint dwNumBCCH = 0;
        }


        // -------------------------------------------------------------------
        //  RIL DLL functions 
        // -------------------------------------------------------------------

        /* Definition from: http://msdn2.microsoft.com/en-us/library/aa919106.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_Initialize(uint dwIndex,
                                                    RILRESULTCALLBACK pfnResult,
                                                    RILNOTIFYCALLBACK pfnNotify,
                                                    uint dwNotificationClasses,
                                                    uint dwParam,
                                                    out IntPtr lphRil);

        /* Definition from: http://msdn2.microsoft.com/en-us/library/aa923065.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_GetCellTowerInfo(IntPtr hRil);

        /* Definition from: http://msdn2.microsoft.com/en-us/library/aa919624.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_Deinitialize(IntPtr hRil);
    }
}
