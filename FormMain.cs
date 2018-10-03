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

        private Dictionary<string, string> GenerateRequest(string[,] teams, List<string> available)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            int team_no;
            if (radioButtonTeam0.Checked)
                team_no = 0;
            else
                team_no = 1;
            string json = JsonConvert.SerializeObject(team_no);
            dict.Add("team_no", json);

            json = JsonConvert.SerializeObject(teams);
            dict.Add("teams", json);

            json = JsonConvert.SerializeObject(available);
            dict.Add("available", json);

            ServerConfig cfg = new ServerConfig();
            cfg.config_str = CustomData["dotamax_cn"]["server"][comboBoxServer.Text] +
                "_" + CustomData["dotamax_cn"]["skill"][comboBoxSkill.Text] +
                "_" + CustomData["dotamax_cn"]["ladder"][comboBoxLadder.Text];
            cfg.roles = new List<string>();
            for (int i = 0; i < checkedListBoxRole.Items.Count; i++)
            {
                if (checkedListBoxRole.GetItemChecked(i))
                {
                    string name = checkedListBoxRole.Items[i].ToString();
                    string roleName = RoleName[name].ToString();
                    cfg.roles.Add(roleName);
                }
            }
            json = JsonConvert.SerializeObject(cfg);
            dict.Add("cfg", json);

            return dict;
        }

        void SetLabel(string[,] teamText)
        {
            for (int t = 0; t < 2; t++)
            {
                for (int n = 0; n < 5; n++)
                {
                    int index = 1 + n + 5 * t;
                    object obj = this.GetType().GetField("label" + index,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
                    Label labelBox = (Label)obj;
                    labelBox.Text = teamText[t, n];
                }
            }
        }

        private void btnScreenPredict_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> available = imgProc.GetAvailableHero();
                if (available.Count <= 30)
                {
                    throw new Exception();
                }
                string[,] teamText;
                string[,] teams = imgProc.GetChosenHero(available, out teamText);
                SetLabel(teamText);
                Dictionary<string, string> dict = GenerateRequest(teams, available);
                Request(dict);
            }
            catch (Exception ex)
            {
                textBoxOutput.Text = "错误, 请检查是否已经按照使用手册里面描述的方法进行截图.";
            }
        }

        void Request(Dictionary<string, string> dict)
        {
            string result = Post(Util.Base64Decode("aHR0cDovLzk3LjY0LjQ0LjE5Mjo=") + "30207/bp", dict);
            //string result = Post("http://127.0.0.1:30207/bp", dict);
            JObject res = JsonConvert.DeserializeObject<JObject>(result);
            var table = (JArray)res["table"];
            dataGridViewTable.Rows.Clear();
            dataGridViewTable.Refresh();
            dataGridViewTable.Rows.Add(table.Count);
            for (int i = 0; i < table.Count; i++)
            {
                var line = (JArray)table[i];
                for (int j = 0; j < line.Count; j++)
                {
                    dataGridViewTable.Rows[i].Cells[j].Value = line[j].ToString();
                }
            }
            string output = (string)((JValue)res["output"]).Value;
            textBoxOutput.Text = output;
        }

        void AddAutoComplete()
        {
            AutoCompleteStringCollection strings = new AutoCompleteStringCollection();
            Dictionary<string, List<string>> abbr = CustomData["abbrev_dict"].ToObject<Dictionary<string, List<string>>>();
            foreach (var h in abbr.Values)
            {
                strings.AddRange(h.ToArray());
            }
            for (int i = 1; i <= 10; i++)
            {
                object obj = this.GetType().GetField("textBox" + i,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
                TextBox txtBox = (TextBox)obj;
                txtBox.AutoCompleteCustomSource = strings;
            }
        }

        void SetDefault()
        {
            comboBoxServer.SelectedIndex = 0;
            comboBoxSkill.SelectedIndex = 0;
            comboBoxLadder.SelectedIndex = 0;

            SetAllRoles(true);
        }

        void SetAllRoles(bool flag)
        {
            for (int i = 0; i < checkedListBoxRole.Items.Count; i++)
            {
                checkedListBoxRole.SetItemChecked(i, flag);
            }
        }

        JObject CustomData;
        ImageProc imgProc;
        JObject RoleName;
        private void FormMain_Load(object sender, EventArgs e)
        {
            CustomData = JsonConvert.DeserializeObject<JObject>(Util.GetFromResources("custom_data.json"));
            imgProc = new ImageProc(CustomData);
            AddAutoComplete();
            SetDefault();
            var array = (JArray)CustomData["role"]["role_name"];
            RoleName = (JObject)array[1];
        }

        private void checkBoxUnlock_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUnlock.Checked)
            {
                for (int i = 1; i <= 10; i++)
                {
                    object obj = this.GetType().GetField("textBox" + i,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
                    TextBox txtBox = (TextBox)obj;
                    txtBox.Enabled = true;
                }
                btnTextPredict.Enabled = true;
            }
            else
            {
                for (int i = 1; i <= 10; i++)
                {
                    object obj = this.GetType().GetField("textBox" + i,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
                    TextBox txtBox = (TextBox)obj;
                    txtBox.Enabled = false;
                }
                btnTextPredict.Enabled = false;
            }
        }

        private void btnTextPredict_Click(object sender, EventArgs e)
        {
            string[,] teams = new string[2, 5];
            string[,] teamText = new string[2, 5];
            var available = CustomData["cn_layout"].ToObject<List<string>>();
            for (int t = 0; t < 2; t++)
            {
                for (int n = 0; n < 5; n++)
                {
                    int index = 1 + n + t * 5;
                    object obj = this.GetType().GetField("textBox" + index,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
                    TextBox txtBox = (TextBox)obj;
                    if (txtBox.Text == "无" || txtBox.Text == "none" || txtBox.Text == "")
                    {
                        teams[t, n] = "none";
                        teamText[t, n] = "无";
                    }
                    else
                    {
                        var _dict = CustomData["inverse_abbrev_dict"].ToObject<Dictionary<string, string>>();
                        if (_dict.ContainsKey(txtBox.Text))
                        {
                            var key = _dict[txtBox.Text];
                            teams[t, n] = key;
                            teamText[t, n] = CustomData["abbrev_dict"][key].ToObject<List<string>>().Last();
                            available.Remove(key);
                        }
                        else
                        {
                            teams[t, n] = "none";
                            teamText[t, n] = "无";
                        }
                        
                    }
                }
            }
            SetLabel(teamText);
            Dictionary<string, string> dict = GenerateRequest(teams, available);
            Request(dict);
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            SetAllRoles(true);
        }

        private void buttonSelectNone_Click(object sender, EventArgs e)
        {
            SetAllRoles(false);
        }
    }
}
