using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotaplus_desktop
{
    public class ImageProc
    {
        JObject CustomData;
        List<string> Heroes = new List<string>();
        Dictionary<string, Image<Bgr, byte>> TemplateDict = new Dictionary<string, Image<Bgr, byte>>();
        Dictionary<string, Image<Bgr, byte>> UpTemplateDict = new Dictionary<string, Image<Bgr, byte>>();

        public ImageProc(JObject customData)
        {
            CustomData = customData;
            LoadTemplate();
        }

        void LoadTemplate()
        {
            foreach (JValue hero in (JArray)CustomData["cn_layout"])
            {
                string heroName = (string)hero.Value;
                Heroes.Add(heroName);
                Image<Bgr, byte> template = new Image<Bgr, byte>(Util.GetImageFromResources("res.hero." + heroName + ".png"));
                TemplateDict.Add(heroName, template);
                Image<Bgr, byte> upTemplate = new Image<Bgr, byte>(Util.GetImageFromResources("res.hero_up." + heroName + ".png"));
                UpTemplateDict.Add(heroName, upTemplate);
            }
        }

        public static Bitmap GetScreenshot()
        {
            Bitmap image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics imgGraphics = Graphics.FromImage(image))
            {
                //设置截屏区域
                imgGraphics.CopyFromScreen(0, 0, 0, 0, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            }
            return image;
        }

        public static Bitmap GetScreenshotFromClipboard()
        {
            Bitmap image;
            IDataObject data = Clipboard.GetDataObject();
            image = (Bitmap)(data.GetData(typeof(Bitmap)));
            return image;
        }

        public List<string> GetAvailableHero()
        {
            List<string> available = new List<string>();
            Image<Bgr, byte> screen = new Image<Bgr, byte>(ImageProc.GetScreenshotFromClipboard());
            PosIndex posIndex = new PosIndex((JObject)CustomData["hero_choose_interface"]);
            foreach (string heroName in Heroes)
            {
                posIndex.Increment();
                var pos = posIndex.GetPos();
                if (DetectOneHero(pos, heroName, screen))
                    available.Add(heroName);
            }
            return available;
        }

        public string[,] GetChosenHero(List<string> available, out string[,] teamText)
        {
            List<string> deepCopiedList = GenericCopier<List<string>>.DeepCopy(Heroes);
            var _heroes = deepCopiedList.Except(available).ToList();
            Image<Bgr, byte> screen = new Image<Bgr, byte>(ImageProc.GetScreenshotFromClipboard());
            string[,] teams = new string[2, 5];
            teamText = new string[2, 5];
            for (int t = 0; t < 2; t++)
            {
                for (int n = 0; n < 5; n++)
                {
                    string res = DetectOneHeroUp(t, n, _heroes, screen);
                    teams[t, n] = res;
                    if (res == "none")
                        teamText[t, n] = "无";
                    else
                    {
                        string name = CustomData["abbrev_dict"][res].ToObject<List<string>>().Last();
                        teamText[t, n] = name;
                    }
                }
            }
            return teams;
        }

        public bool DetectOneHero(List<int> pos, string heroName, Image<Bgr, byte> screen)
        {
            List<int> xy = CustomData["hero_choose_interface"]["x, y"].ToObject<List<int>>();
            List<int> wh = CustomData["hero_choose_interface"]["w, h"].ToObject<List<int>>();
            List<int> d = CustomData["hero_choose_interface"]["d_col, d_row, d_class"].ToObject<List<int>>();
            int x = xy[0] + pos[2] * d[0];
            int y = xy[1] + pos[1] * d[1] + pos[0] * d[2];
            Rectangle roi = new Rectangle(x, y, wh[0], wh[1]);

            screen.ROI = roi;
            var part = screen.Copy();
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

        public string DetectOneHeroUp(int team, int heroNum, List<string> heroes, Image<Bgr, byte> screen)
        {
            int x, y;
            y = CustomData["hero_up_image"]["t_y"].ToObject<int>();
            if (team == 0)
            {
                x = CustomData["hero_up_image"]["t_0_x"].ToObject<int>();
            }
            else
            {
                x = CustomData["hero_up_image"]["t_1_x"].ToObject<int>();
            }
            int d = CustomData["hero_up_image"]["d_hero"].ToObject<int>();
            x += heroNum * d;
            List<int> wh = CustomData["hero_up_image"]["w, h"].ToObject<List<int>>();
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
    }
}
