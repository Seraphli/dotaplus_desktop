using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Newtonsoft.Json.Linq;

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

            var available = GetAvailableHero();
            json = JsonConvert.SerializeObject(available);
            dict.Add("available", json);

            string[,] teams = GetChosenHero(available);
            json = JsonConvert.SerializeObject(teams);
            dict.Add("teams", json);
            return dict;
        }


        private void btnPredict_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> dict = GenerateRequest();
            string result = Post("http://97.64.44.192:30207/bp", dict);
            JObject res = JsonConvert.DeserializeObject<JObject>(result);
            if (res.ContainsKey("g_table_1"))
            {
                string table = (string)((JValue)res["g_table_1"]).Value;
                table = table.Replace("\n", "\r\n");
                textBoxTable.Text = table;
            }
            else
            {
                string table = (string)((JValue)res["g_table"]).Value;
                table = table.Replace("\n", "\r\n");
                textBoxTable.Text = table;
            }
            string output = (string)((JValue)res["output"]).Value;
            textBoxOutput.Text = output;
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

        internal static Bitmap GetImageFromResources(string resourceName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            Bitmap image;
            using (Stream stream = assem.GetManifestResourceStream(assem.GetName().Name + '.' + resourceName))
            {
                image = new Bitmap(stream);
            }
            return image;
        }

        internal static Bitmap GetScreenshotFromClipboard()
        {
            Bitmap image;
            IDataObject data = Clipboard.GetDataObject();
            image = (Bitmap)(data.GetData(typeof(Bitmap)));
            return image;
        }

        internal static Bitmap GetScreenshot()
        {
            Bitmap image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics imgGraphics = Graphics.FromImage(image))
            {
                //设置截屏区域
                imgGraphics.CopyFromScreen(0, 0, 0, 0, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            }
            return image;
        }

        bool DetectOneHero(List<int> pos, string heroName, Image<Bgr, byte> screen)
        {
            List<int> xywh = CustomData["hero_choose_interface"]["X, Y, W, H"].ToObject<List<int>>();
            List<int> d = CustomData["hero_choose_interface"]["D_COL, D_ROW, D_CLASS"].ToObject<List<int>>();
            int x = xywh[0] + pos[2] * d[0];
            int y = xywh[1] + pos[1] * d[1] + pos[0] * d[2];
            Rectangle roi = new Rectangle(x, y, xywh[2], xywh[3]);

            screen.ROI = roi;
            var part = screen.Copy();
            part.Save("Temp.png");
            Image<Gray, float> result = new Image<Gray, float>(part.Width, part.Height);
            result = screen.MatchTemplate(TemplateDict[heroName], TemplateMatchingType.SqdiffNormed);
            double min = 0, max = 0;
            Point maxp = new Point(0, 0);
            Point minp = new Point(0, 0);
            CvInvoke.MinMaxLoc(result, ref min, ref max, ref minp, ref maxp);
            if (min < 0.01)
                return true;
            else
                return false;
        }

        string DetectOneHeroUp(int team, int heroNum, List<string> heroes, Image<Bgr, byte> screen)
        {
            int x, y;
            if (team == 0)
            {
                List<int> xy = CustomData["hero_image"]["T_0_X, T_0_Y"].ToObject<List<int>>();
                x = xy[0];
                y = xy[1];
            }
            else
            {
                List<int> xy = CustomData["hero_image"]["T_1_X, T_1_Y"].ToObject<List<int>>();
                x = xy[0];
                y = xy[1];
            }
            int d = CustomData["hero_image"]["D_HERO"].ToObject<int>();
            x += heroNum * d;
            List<int> wh = CustomData["hero_image"]["UP_W, UP_H"].ToObject<List<int>>();
            Rectangle roi = new Rectangle(x, y, wh[0], wh[1]);

            screen.ROI = roi;
            var part = screen.Copy();
            foreach (string hero in heroes)
            {
                Image<Gray, float> result = new Image<Gray, float>(part.Width, part.Height);
                result = screen.MatchTemplate(UpTemplateDict[hero], TemplateMatchingType.SqdiffNormed);
                double min = 0, max = 0;
                Point maxp = new Point(0, 0);
                Point minp = new Point(0, 0);
                CvInvoke.MinMaxLoc(result, ref min, ref max, ref minp, ref maxp);
                if (min < 0.01)
                    return hero;
            }
            return "none";

        }

        List<string> GetAvailableHero()
        {
            List<string> available = new List<string>();
            Image<Bgr, byte> screen = new Image<Bgr, byte>(GetScreenshotFromClipboard());
            PosIndex posIndex = new PosIndex((JObject)CustomData["hero_choose_interface"]);
            List<int> xywh = CustomData["hero_choose_interface"]["X, Y, W, H"].ToObject<List<int>>();
            List<int> d = CustomData["hero_choose_interface"]["D_COL, D_ROW, D_CLASS"].ToObject<List<int>>();
            foreach (string heroName in heroes)
            {
                posIndex.Increment();
                var pos = posIndex.GetPos();
                if (DetectOneHero(pos, heroName, screen))
                    available.Add(heroName);
            }
            return available;
        }

        string[,] GetChosenHero(List<string> available)
        {
            List<string> deepCopiedList = GenericCopier<List<string>>.DeepCopy(heroes);
            var _heroes = deepCopiedList.Except(available).ToList();
            Image<Bgr, byte> screen = new Image<Bgr, byte>(GetScreenshotFromClipboard());
            string[,] teams = new string[2, 5];
            for (int t = 0; t < 2; t++)
            {
                List<string> teamText = new List<string>();
                for (int n = 0; n < 5; n++)
                {
                    string res = DetectOneHeroUp(t, n, _heroes, screen);
                    teams[t, n] = res;
                    if (res == "none")
                        teamText.Add("无");
                    else
                    {
                        string name = CustomData["CN_ABBREV_DICT"][res].ToObject<List<string>>().Last();
                        teamText.Add(name);
                    }
                }
                if (t == 0)
                    textBoxTeam0.Text = string.Join("  |  ", teamText);
                else
                    textBoxTeam1.Text = string.Join("  |  ", teamText);
            }
            return teams;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var a = GetAvailableHero();
            Console.WriteLine(GetAvailableHero().ToString());
        }

        List<string> heroes = new List<string>();
        Dictionary<string, Image<Bgr, byte>> TemplateDict = new Dictionary<string, Image<Bgr, byte>>();
        Dictionary<string, Image<Bgr, byte>> UpTemplateDict = new Dictionary<string, Image<Bgr, byte>>();
        void LoadTemplate()
        {
            foreach (JValue hero in (JArray)CustomData["CN_LAYOUT"])
            {
                string heroName = (string)hero.Value;
                heroes.Add(heroName);
                Image<Bgr, byte> template = new Image<Bgr, byte>(GetImageFromResources("res.hero." + heroName + ".png"));
                TemplateDict.Add(heroName, template);
                Image<Bgr, byte> upTemplate = new Image<Bgr, byte>(GetImageFromResources("res.hero_up." + heroName + ".png"));
                UpTemplateDict.Add(heroName, upTemplate);
            }
        }

        JObject CustomData;
        private void FormMain_Load(object sender, EventArgs e)
        {
            CustomData = JsonConvert.DeserializeObject<JObject>(FormMain.GetFromResources("custom_data.json"));
            LoadTemplate();
        }
    }
}
