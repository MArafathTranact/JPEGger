using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using System.Net.Security;
using System.Net.Http.Headers;

namespace JPEGgerServer
{
    public class ClientHandler
    {
        public Socket client = null;
        private static readonly Encoding encoding = Encoding.UTF8;
        readonly ClientTransaction clientTransaction = new ClientTransaction();
        public ClientHandler(Socket clientSocket)
        {
            client = clientSocket;
        }

        #region Handler

        public async void HandleRequestAsync()
        {
            try
            {
                while (client != null && client.Connected)
                {
                    try
                    {
                        byte[] bytes = new byte[client.ReceiveBufferSize];
                        int byteCount = clientTransaction.Receive(client, bytes, 0, client.ReceiveBufferSize, 1000);
                        if (byteCount > 0)
                        {
                            LogEvents($" Processing Jpegger request ");

                            string request = Utilities.ByteToHexa(bytes.Take(byteCount).ToArray());

                            var command = request.Split(' ')[0].Trim();
                            LogEvents($" Processing  : {request}");

                            await ParseJPEGgerRequest(request, command);
                        }

                    }
                    catch (Exception ex)
                    {
                        LogEvents($" Exception at ClientHandler.HandleRequest {ex.Message }");
                        SendFailedStatus();
                    }
                }
                if (client != null && !client.Connected)
                    StopClient();

            }
            catch (Exception ex)
            {

                LogEvents($" Exception at ClientHandler.HandleRequest {ex.Message }");
                SendFailedStatus();
            }

        }

        #endregion

        #region General 
        private async Task ParseJPEGgerRequest(string request, string command)
        {
            var response = string.Empty;

            switch (command)
            {
                case "capture":
                    response = ProcessCaptureCommand(command, request);
                    break;
                case "map":
                    response = await ProcessMapCommand(command, request);
                    break;
                case "copyimage":
                    response = ProcessCopyImageCommand(command, request);
                    break;
                case "void":
                    response = ProcessVoidCommand(command, request);
                    break;
            }

            byte[] sendData = Utilities.GetSendBytes(response);
            clientTransaction.Send(client, sendData, 0, sendData.Length, 10000);

        }
        private void SendFailedStatus()
        {
            //byte[] sendData = Utilities.GetSendBytes("FAILED");
            //clientTransaction.Send(client, sendData, 0, sendData.Length, 10000);
            StopClient();

        }
        public void StopClient()
        {
            try
            {
                if (client != null)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    client = null;
                }

            }
            catch (Exception ex)
            {
                LogEvents($" Exception at ClientHandler.StopClient {ex.Message }");
            }
        }
        public static string GetAppSettingValue(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
        private void LogEvents(string input)
        {
            Logger.LogWithNoLock($"{DateTime.Now:MM-dd-yyyy HH:mm:ss}: {Thread.CurrentThread.ManagedThreadId} : {input}");
        }


        #endregion

        #region CameraCapture
        private string ProcessCaptureCommand(string command, string request)
        {
            try
            {
                var splittedRequest = request.Replace(command, "").Split('>');
                var jpeggerRequest = new JpeggerCameraCaptureRequest();
                foreach (var item in splittedRequest)
                {
                    var split = item.Split(new string[] { "=<" }, StringSplitOptions.None);
                    var filter = split[0].Trim().ToLower();
                    switch (filter)
                    {
                        case var s when filter.Contains("camera_name"):
                            jpeggerRequest.CameraName = split[1];
                            break;
                        case var s when filter.Contains("amount"):
                            jpeggerRequest.Amount = split[1];
                            break;
                        case var s when filter.Contains("receipt_nbr"):
                            jpeggerRequest.ReceiptNumber = int.Parse(split[1]);
                            break;
                        case var s when filter.Contains("location"):
                            jpeggerRequest.Location = split[1];
                            break;
                        case var s when filter.Contains("event_code"):
                            jpeggerRequest.EventCode = split[1];
                            break;
                        case var s when filter.Contains("cust_nbr"):
                            jpeggerRequest.CustomerNumber = split[1];
                            break;
                        case var s when filter.Contains("cert_nbr"):
                            jpeggerRequest.CertificationNumber = split[1];
                            break;
                        case var s when filter.Contains("last_name"):
                            jpeggerRequest.CustomerLastName = split[1];
                            break;
                        case var s when filter.Contains("first_name"):
                            jpeggerRequest.CustomerFirstName = split[1];
                            break;
                        case var s when filter.Contains("ticket_nbr"):
                            jpeggerRequest.TicketNumber = split[1];
                            break;
                        case var s when filter.Contains("tare_seq_nbr"):
                            jpeggerRequest.TareSequenceNumber = split[1];
                            break;
                        case var s when filter.Contains("cust_name"):
                            jpeggerRequest.CustomerName = split[1];
                            break;
                        case var s when filter.Contains("cmdy_name"):
                            jpeggerRequest.CommodityName = split[1];
                            break;
                        case var s when filter.Contains("weight"):
                            jpeggerRequest.Weight = split[1];
                            break;
                        case var s when filter.Contains("table"):
                            jpeggerRequest.TableName = split[1];
                            break;
                        case var s when filter.Contains("man_weight_flag"):
                            jpeggerRequest.IsManual = int.Parse(split[1]);
                            break;
                        case var s when filter.Contains("cmdy_nbr"):
                            jpeggerRequest.CommodityNumber = split[1];
                            break;
                        case var s when filter.Contains("branch_code"):
                            jpeggerRequest.BranchCode = split[1];
                            break;
                        case var s when filter.Contains("transaction_type"):
                            jpeggerRequest.TransactionType = split[1];
                            break;
                        case var s when filter.Contains("initials"):
                            jpeggerRequest.Initials = split[1];
                            break;
                        case var s when filter.Contains("app_date_time"):
                            jpeggerRequest.AppDateTime = split[1];
                            break;
                        case var s when filter.Contains("contr_name"):
                            jpeggerRequest.ContractName = split[1];
                            break;
                        case var s when filter.Contains("contr_nbr"):
                            jpeggerRequest.ContractNumber = split[1];
                            break;
                        case var s when filter.Contains("container_nbr"):
                            jpeggerRequest.ContainerNumber = split[1];
                            break;
                        case var s when filter.Contains("contract_name"):
                            jpeggerRequest.Contract_Number = split[1];
                            break;
                        case var s when filter.Contains("routingnumber"):
                            jpeggerRequest.RoutingNumber = split[1];
                            break;
                        case var s when filter.Contains("accountnumber"):
                            jpeggerRequest.AccountNumber = split[1];
                            break;
                        case var s when filter.Contains("checknumber"):
                            jpeggerRequest.CheckNumber = split[1];
                            break;
                        case var s when filter.Contains("ticket_type"):
                            jpeggerRequest.TicketType = split[1];
                            break;

                    }

                }

                if (string.IsNullOrEmpty(jpeggerRequest.TableName))
                    jpeggerRequest.TableName = "images";

                jpeggerRequest.YardId = Guid.Parse(GetAppSettingValue("YardId"));

                var camera = Camera.GetConfiguredCamera(jpeggerRequest.CameraName);
                if (camera != null && camera.Count == 1)
                {
                    Thread captureImage = new Thread(() => CaptureImage(camera.FirstOrDefault(), jpeggerRequest));
                    captureImage.Start();
                    LogEvents($" Camera exist, sending  Success for ticket number {jpeggerRequest.TicketNumber}");
                    return "SUCCESS";
                }
                else if (camera != null && camera.Count > 1)
                {
                    foreach (var item in camera)
                    {
                        Thread captureImage = new Thread(() => CaptureImage(item, jpeggerRequest));
                        captureImage.Start();
                        LogEvents($" Camera exist, sending  Success for ticket number  {jpeggerRequest.TicketNumber}");
                    }

                    return "SUCCESS  ";
                }
                else
                {
                    LogEvents($" Camera doesn't exist. Sending Fail for ticket number {jpeggerRequest.TicketNumber}");
                    return "FAIL  ";
                }
            }
            catch (Exception ex)
            {
                LogEvents($"Exception at ClientHandler.ProcessCaptureCommand, Message :{ex.Message }");
                return "FAIL";
            }

        }

        private async void CaptureImage(Camera camera, JpeggerCameraCaptureRequest request)
        {
            if (camera != null && !string.IsNullOrEmpty(camera.URL))
            {
                var mstream = TakePicture(camera, request.TicketNumber);
                if (mstream != null && mstream.Length > 0)
                {
                    LogEvents($" Image captured for ticket number {request.TicketNumber} from '{camera.Camera_Name}' camera ,length {mstream.Length}");
                    await PostJpeggerImage(mstream, request);
                }
            }
        }

        private MemoryStream TakePicture(Camera camera, string ticketNumber)
        {
            try
            {
                LogEvents($" Capturing Image for ticket number {ticketNumber} from '{camera.Camera_Name}' camera ");
                var req = WebRequest.Create(camera.URL);
                req.Timeout = 6000;
                if (!string.IsNullOrEmpty(camera.Username) && !string.IsNullOrEmpty(camera.Pwd))
                    req.Credentials = new NetworkCredential(camera.Username, camera.Pwd);
                req.Method = "GET";
                var res = req.GetResponse();
                Stream stream = res.GetResponseStream();


                byte[] buffer = new byte[5000000];
                int read, total = 0;
                while ((read = stream.Read(buffer, total, 1000)) != 0)
                {
                    total += read;
                }

                return new MemoryStream(buffer, 0, total);
            }
            catch (Exception ex)
            {
                LogEvents($" Failed to capture image from '{camera.Camera_Name}' camera, Exception Message :{ex.Message }");
                return null;
            }
        }

        private async Task<bool> PostJpeggerImage(MemoryStream img, JpeggerCameraCaptureRequest request)
        {
            try
            {
                LogEvents($" Start saving captured image for the ticket '{request.TicketNumber}' , image length ={img.Length} ");
                var formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
                var contentType = "multipart/form-data; boundary=" + formDataBoundary;
                var tableName = request.TableName.ToLowerInvariant();
                var formData = await GenerateMultipartFormData(formDataBoundary, img, request, tableName);
                if (formData != null)
                {
                    return await PostMultiForm(contentType, formData, tableName, request.TicketNumber);
                }
            }
            catch (Exception ex)
            {
                LogEvents($" Failed to save the captured image for ticket  '{request.TicketNumber}', Exception Message :{ex.Message }");
            }

            return false;
        }

        private async Task<byte[]> GenerateMultipartFormData(string boundary, MemoryStream image, JpeggerCameraCaptureRequest request, string table)
        {
            try
            {
                using (var formDataStream = new MemoryStream())
                {
                    LogEvents($" Generating multipart data for ticket '{request.TicketNumber}' ");
                    var needNewLine = false;
                    table = table.TrimEnd('s').ToLowerInvariant();
                    var param = string.Empty;
                    foreach (var prop in request.GetType().GetProperties())
                    {
                        if (needNewLine)
                        {
                            await formDataStream.WriteAsync(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
                        }

                        switch (prop.Name)
                        {
                            case "TicketNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[ticket_nbr]", prop.GetValue(request)?.ToString());
                                break;
                            case "CameraName":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[camera_name]", prop.GetValue(request)?.ToString());
                                break;
                            case "EventCode":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[event_code]", prop.GetValue(request)?.ToString());
                                break;
                            case "ReceiptNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[receipt_nbr]", prop.GetValue(request)?.ToString());
                                break;
                            case "Location":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[location]", prop.GetValue(request)?.ToString());
                                break;
                            case "TareSequenceNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[tare_seq_nbr]", prop.GetValue(request)?.ToString());
                                break;
                            case "Amount":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[amount]", prop.GetValue(request)?.ToString());
                                break;
                            case "ContractNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[contr_nbr]", prop.GetValue(request)?.ToString());
                                break;
                            case "ContractName":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[contr_name]", prop.GetValue(request)?.ToString());
                                break;
                            case "Weight":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[weight]", prop.GetValue(request)?.ToString());
                                break;
                            case "CustomerName":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[cust_name]", prop.GetValue(request)?.ToString());
                                break;
                            case "CustomerNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[cust_nbr]", prop.GetValue(request)?.ToString());
                                break;
                            case "BranchCode":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[branch_code]", prop.GetValue(request)?.ToString());
                                break;
                            case "CommodityName":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[cmdy_name]", prop.GetValue(request)?.ToString());
                                break;
                            case "CommodityNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[cmdy_nbr]", prop.GetValue(request)?.ToString());
                                break;
                            case "Initials":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[initials]", prop.GetValue(request)?.ToString());
                                break;
                            case "TransactionType":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[transaction_type]", prop.GetValue(request)?.ToString());
                                break;
                            case "IsManual":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[man_weight_flag]", prop.GetValue(request)?.ToString());
                                break;
                            case "AppDateTime":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[app_date_time]", prop.GetValue(request)?.ToString());
                                break;
                            case "ContainerNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[container_nbr]", prop.GetValue(request)?.ToString());
                                break;
                            case "Contract_Number":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[contract_name]", prop.GetValue(request)?.ToString());
                                break;
                            case "RoutingNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[routingnumber]", prop.GetValue(request)?.ToString());
                                break;
                            case "AccountNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[accountnumber]", prop.GetValue(request)?.ToString());
                                break;
                            case "CheckNumber":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[checknumber]", prop.GetValue(request)?.ToString());
                                break;
                            case "TicketType":
                                param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[ticket_type]", prop.GetValue(request)?.ToString());
                                break;
                            default:
                                needNewLine = false;
                                param = string.Empty;
                                break;
                        }
                        if (!string.IsNullOrEmpty(param))
                        {
                            await formDataStream.WriteAsync(encoding.GetBytes(param), 0, encoding.GetByteCount(param));
                            needNewLine = true;
                        }
                    }

                    if (formDataStream.Length != 0)
                    {
                        await formDataStream.WriteAsync(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
                    }

                    param = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, $"{table}[yardid]", request.YardId);
                    if (!string.IsNullOrEmpty(param))
                    {
                        await formDataStream.WriteAsync(encoding.GetBytes(param), 0, encoding.GetByteCount(param));
                    }

                    param = string.Empty;

                    if (formDataStream.Length != 0)
                    {
                        await formDataStream.WriteAsync(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
                    }

                    byte[] img = image != null ? image.ToArray() : new byte[0];
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n", boundary, $"{table}[file]", "display.jpg", "application/octet-stream");
                    await formDataStream.WriteAsync(encoding.GetBytes(header), 0, encoding.GetByteCount(header));
                    await formDataStream.WriteAsync(img, 0, img.Length);

                    string footer = "\r\n--" + boundary + "--\r\n";
                    await formDataStream.WriteAsync(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));
                    formDataStream.Position = 0;

                    byte[] formData = new byte[formDataStream.Length];
                    await formDataStream.ReadAsync(formData, 0, formData.Length);
                    return formData;
                }
            }
            catch (Exception ex)
            {
                LogEvents($" Failed to generate multipart data for ticket  '{request.TicketNumber}', Exception Message :{ex.Message }");
                return null;
            }
        }

        public async Task<bool> PostMultiForm(string contentType, byte[] formData, string table, string ticketNumber)
        {
            var status = false;
            try
            {
                LogEvents($" Saving captured image to '{table}' table");
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var request = WebRequest.Create(GetAppSettingValue("JPEGgerAPI") + table);
                request.Method = "POST";
                request.ContentType = contentType;
                request.ContentLength = formData.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    await requestStream.WriteAsync(formData, 0, formData.Length);
                }
                using (var response = await request.GetResponseAsync())
                {
                    if (request.GetResponse() is HttpWebResponse webresponse && webresponse.StatusCode == HttpStatusCode.Created)
                    {
                        Stream stream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(stream);
                        var result = reader.ReadToEnd();
                        //Capture_seq_nbr = JsonConvert.DeserializeObject<CapturedImageResponse>(result).Capture_Seq_Nbr;
                        status = true;

                        LogEvents($" Image posted succesfully to '{table}' table");
                    }
                    else
                        status = false;
                }
                return status;
            }
            catch (Exception ex)
            {
                LogEvents($" Failed to save the captured image for ticket  '{ticketNumber}', Exception Message :{ex.Message }");
                return status;
            }
        }

        #endregion

        #region Map
        private async Task<string> ProcessMapCommand(string command, string request)
        {
            try
            {
                var splittedRequest = request.Replace(command, "").Split('>');
                List<string> ticketNumbers = new List<string>();
                List<string> receiptNumbers = new List<string>();
                List<bool> mapStatus = new List<bool>();
                foreach (var item in splittedRequest)
                {
                    var split = item.Split(new string[] { "=<" }, StringSplitOptions.None);
                    var filter = split[0].Trim();
                    switch (filter)
                    {
                        case var s when filter.Contains("ticket_nbr"):
                            ticketNumbers.Add(split[1]);
                            break;
                        case var s when filter.Contains("receipt_nbr"):
                            receiptNumbers.Add(split[1]);
                            break;
                    }
                }

                if (receiptNumbers.Count == 1)
                {
                    foreach (var ticketNumber in ticketNumbers)
                    {
                        var jpeggerMap = new JpeggerMap() { ticket_nbr = ticketNumber, receipt_nbr = receiptNumbers.FirstOrDefault(), yardid = GetAppSettingValue("YardId"), date_time = DateTime.UtcNow };

                        var result = await CreateRTLookUp(jpeggerMap);
                        if (result)
                        {
                            mapStatus.Add(true);

                            LogEvents($" Relationship mapped for receipt ='{jpeggerMap.receipt_nbr}' with ticket='{jpeggerMap.ticket_nbr}' ");
                        }
                        else
                        {
                            mapStatus.Add(false);
                            LogEvents($" Relationship mapping failed for receipt ='{jpeggerMap.receipt_nbr}' with ticket='{jpeggerMap.ticket_nbr}' ");

                        }
                    }
                }
                else if (ticketNumbers.Count == 1)
                {
                    foreach (var receiptNumber in receiptNumbers)
                    {
                        var jpeggerMap = new JpeggerMap() { ticket_nbr = ticketNumbers.FirstOrDefault(), receipt_nbr = receiptNumber, yardid = GetAppSettingValue("YardId"), date_time = DateTime.UtcNow };

                        var result = await CreateRTLookUp(jpeggerMap);
                        if (result)
                        {
                            mapStatus.Add(true);
                            LogEvents($" Relationship mapped for  ticket ='{jpeggerMap.ticket_nbr}' with receipt ='{jpeggerMap.receipt_nbr}'");
                        }
                        else
                        {
                            mapStatus.Add(false);
                            LogEvents($" Relationship mapping failed for  ticket ='{jpeggerMap.ticket_nbr}' with receipt ='{jpeggerMap.receipt_nbr}'");

                        }
                    }
                }

                if (mapStatus.Contains(false))
                    return "FAIL";
                else
                    return "SUCCESS";
            }
            catch (Exception ex)
            {

                LogEvents($" Exception at ClientHandler.ProcessMapCommand, Message :{ex.Message }");
                return "FAIL";
            }
        }

        private async Task<bool> CreateRTLookUp(JpeggerMap jpeggerMap)
        {
            var method = "";
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                using (var client = new HttpClient())
                {
                    method = GetAppSettingValue("JPEGgerAPI") + "rt_lookups";
                    client.Timeout = TimeSpan.FromSeconds(5);
                    using (HttpResponseMessage response = await client.PostAsync(method, jpeggerMap, new JsonMediaTypeFormatter()))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }

                    }
                }
            }
            catch (TimeoutException ex)
            {
                LogEvents($" Timed out for mapping ticket '{jpeggerMap.ticket_nbr}' with receipt '{jpeggerMap.receipt_nbr}'");
                return false;
            }
            catch (Exception ex)
            {
                LogEvents($" Exception at ClientHandler.CreateRTLookUp, Message :{ex.Message }");
                return false;
            }
            return true;
        }

        #endregion

        #region CopyImage
        private string ProcessCopyImageCommand(string command, string request)
        {
            try
            {
                var splittedRequest = request.Replace(command, "").Split('>');
                var source = string.Empty;
                var destination = string.Empty;
                var id = string.Empty;

                foreach (var item in splittedRequest)
                {
                    var split = item.Split(new string[] { "=<" }, StringSplitOptions.None);
                    var filter = split[0].Trim();
                    switch (filter)
                    {
                        case "source":
                            source = split[1];
                            break;
                        case "dest":
                            destination = split[1];
                            break;
                        case var s when filter.Contains(source + ".id"):
                            id = split[1];
                            break;

                    }
                }
                LogEvents($" Copying image from {source} to {destination} with id {id}");
                return CopyImage(source, id, destination) ? "SUCCESS" : "FAIL";
            }
            catch (Exception ex)
            {
                LogEvents($" Exception at ClientHandler.ProcessCopyImageCommand, Message :{ex.Message }");
                return "FAIL";
            }

        }

        private bool CopyImage(string source, string id, string destination)
        {
            try
            {
                var method = "";
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    method = GetAppSettingValue("JPEGgerAPI") + source + "/" + id + "/copy?destination=" + destination;
                    using (HttpResponseMessage response = client.GetAsync(method).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            LogEvents($" Copied image from {source} to {destination} with id {id} is success");
                            return true;
                        }
                        else
                        {
                            LogEvents($" Failed to copy image from {source} to {destination} with id {id} . {response.StatusCode}");
                            return false;
                        }
                    }
                }
            }
            catch (TimeoutException ex)
            {
                LogEvents($" Timed out to copy image from {source} to {destination} with id {id}");
                return false;
            }
            catch (Exception ex)
            {
                LogEvents($" Exception at ClientHandler.ProcessCopyImageCommand, Message :{ex.Message }");
                return false;
            }
        }
        #endregion

        #region Void

        private string ProcessVoidCommand(string command, string request)
        {
            try
            {
                var splittedRequest = request.Replace(command, "").Split('>');
                foreach (var item in splittedRequest)
                {

                }

                return "SUCCESS";
            }
            catch (Exception ex)
            {

                LogEvents($" Exception at ClientHandler.ProcessVoidCommand, Message :{ex.Message }");
                return "FAIL";
            }
        }

        #endregion



    }
}
