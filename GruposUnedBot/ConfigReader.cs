namespace GruposUnedBot
{
    class Config
    {
        public string Token { get; private set; }

        public Config(string configFile, string tokenFile)
        {
            Token = System.IO.File.ReadAllText(tokenFile).Trim();
            //System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            //doc.Load(configFile);
            //Token = doc.SelectSingleNode("telegrambot/token").InnerText;
        }

    }
}
