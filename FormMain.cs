using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace dotaplus_desktop
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 指定Post地址使用Get 方式获取全部字符串
        /// </summary>
        /// <param name="url">请求后台地址</param>
        /// <returns></returns>
        public static string Post(string url, Dictionary<string, string> dict)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            #region 添加Post 参数
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dict)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        private Dictionary<string, string> GenerateRequest()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            int team_no = 0;
            string json = JsonConvert.SerializeObject(team_no);
            dict.Add("team_no", json);

            string[,] teams = new string[2, 5];
            for (int i = 0; i < 2; i += 1)
            {
                for (int j = 0; j < 5; j += 1)
                {
                    teams[i, j] = "none";
                }
            }
            json = JsonConvert.SerializeObject(teams);
            dict.Add("teams", json);

            var available = CustomData["CN_LAYOUT"];
            json = JsonConvert.SerializeObject(available);
            dict.Add("available", json);
            return dict;
        }

        private void btnPredict_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> dict = GenerateRequest();
            string result = FormMain.Post("http://127.0.0.1:30207/bp", dict);
            Console.WriteLine(result);
            Newtonsoft.Json.Linq.JObject res = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(result);
            var a = res["h_v"]["abaddon"];
            Console.WriteLine(res["h_v"]["abaddon"]);
        }

        internal static string GetFromResources(string resourceName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();

            using (Stream stream = assem.GetManifestResourceStream(assem.GetName().Name + '.' + resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

       
        private void button1_Click(object sender, EventArgs e)
        {
        }

        Newtonsoft.Json.Linq.JObject CustomData;
        private void FormMain_Load(object sender, EventArgs e)
        {
            CustomData = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(FormMain.GetFromResources("custom_data.json"));
        }
    }
}
