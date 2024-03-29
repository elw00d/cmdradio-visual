﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Binding.Converters;
using Binding.Observables;
using ConsoleFramework;
using ConsoleFramework.Controls;
using Newtonsoft.Json;
using Un4seen.Bass;

namespace cmdradio
{
    /// <summary>
    /// For empty status string returns Visibility.Collapsed, otherwise returns Visibility.Visible.
    /// </summary>
    public class StringToTextBlockVisibilityConverter : IBindingConverter
    {
        public Type FirstType { get { return typeof(String); } }
        public Type SecondType { get { return typeof(Visibility); } }

        public ConversionResult Convert(object first)
        {
            if (string.IsNullOrEmpty((string)first))
                return new ConversionResult(Visibility.Collapsed);
            return new ConversionResult(Visibility.Visible);
        }

        public ConversionResult ConvertBack(object second)
        {
            throw new NotImplementedException();
        }
    }

    class Program
    {
        class PlayerWindowModel : INotifyPropertyChanged
        {
            private ObservableList genres = new ObservableList( new ArrayList() );
            public ObservableList Genres {
                get {
                    return genres;
                }
            }

            private int? selectedGenreIndex;
            public int? SelectedGenreIndex {
                get { return selectedGenreIndex; }
                set {
                    if ( value != selectedGenreIndex ) {
                        selectedGenreIndex = value;
                        OnPropertyChanged( "SelectedGenreIndex" );
                    }
                }
            }

            private int volume = 100;
            
            public int Volume {
                get { return volume; }
                set {
                    if ( value != volume ) {
                        volume = value;
                        OnPropertyChanged( "Volume" );
                    }
                }
            }

            private string status;
            public string Status {
                get { return status; }
                set {
                    if ( status != value ) {
                        status = value;
                        OnPropertyChanged( "Status" );
                    }
                }
            }

            private string status2;
            public string Status2 {
                get { return status2; }
                set {
                    if ( status2 != value ) {
                        status2 = value;
                        OnPropertyChanged( "Status2" );
                    }
                }
            }

            private string status3;
            public string Status3 {
                get { return status3; }
                set {
                    if ( status3 != value ) {
                        status3 = value;
                        OnPropertyChanged( "Status3" );
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged( string propertyName ) {
                if ( ConsoleApplication.Instance.IsUiThread( ) ) {
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
                } else {
                    Action action = new Action(( ) => {
                        PropertyChangedEventHandler handler = PropertyChanged;
                        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                    });
                    ConsoleApplication.Instance.Post( action );
                }
            }
        }

        private static void splitStatus( String status, PlayerWindowModel model ) {
            string[ ] strings = status.Split( '\n' );
            model.Status = model.Status2 = model.Status3 = string.Empty;
            if ( strings.Length > 0 ) {
                model.Status = strings[ 0 ];
            }
            if ( strings.Length > 1 ) {
                model.Status2 = strings[ 1 ];
            }
            if ( strings.Length > 2 ) {
                model.Status3 = strings[ 2 ];
            }
        }

        static void Main(string[] args)
        {
            WindowsHost windowsHost = ( WindowsHost ) ConsoleApplication.LoadFromXaml( "cmdradio.WindowsHost.xml", null );

            PlayerWindowModel playerWindowModel = new PlayerWindowModel(  );
            Window playerWindow = (Window)ConsoleApplication.LoadFromXaml("cmdradio.PlayerWindow.xml", playerWindowModel);
            Player player = new Player(  );
            //player.ReadCmd(new string[] { "shoutcast" });
            playerWindow.FindChildByName< Button >( "buttonPlay" ).OnClick += ( sender, eventArgs ) => {
                int? selectedGenreIndex = playerWindowModel.SelectedGenreIndex;
                if ( selectedGenreIndex.HasValue ) {
                    new Thread( ( ( ) => {
                        player.cmd = new string[ ]
                            { "play", ( string ) playerWindowModel.Genres[ selectedGenreIndex.Value ] };
                        player.Play( );
                    } ) ).Start( );
                }
            };
            playerWindow.FindChildByName< Button >( "buttonPause" ).OnClick += ( sender, eventArgs ) => {
                new Thread( () => player.ReadCmd( new string[] {"pause"} ) ).Start();
            };
            playerWindow.FindChildByName< Button >( "buttonStop" ).OnClick += ( sender, eventArgs ) => {
                new Thread(() => player.ReadCmd(new string[] { "stop" })).Start();
            };
            playerWindow.FindChildByName< Button >( "buttonExit" ).OnClick += ( sender, eventArgs ) => {
                ConsoleApplication.Instance.Exit( );
            };
            windowsHost.Show( playerWindow );
            foreach ( string s in player.GetGenres( )) {
                playerWindowModel.Genres.Add( s );
            }

            playerWindowModel.PropertyChanged += ( sender, eventArgs ) => {
                if ( eventArgs.PropertyName == "Volume" ) {
                    player.ReadCmd( new string[]
                        {
                            "volume",
                            string.Format( "{0}", playerWindowModel.Volume )
                        });
                }
            };
            splitStatus(player.Status, playerWindowModel);
            player.PropertyChanged += ( sender, eventArgs ) => {
                if ( eventArgs.PropertyName == "Status" ) {
                    splitStatus(player.Status, playerWindowModel);
                }
            };

            ConsoleApplication.Instance.Run(windowsHost);

//            Player player = new Player();
//            string[] p = { };
//            Console.WriteLine("cmdradio v"+Player.VERSION+" by Mitrich Kasus.\nVisit http://cmdradio.org for details.\nFollow @cmdradio and +cmdradio for updates.\n");
//            Console.WriteLine("Type 'genre' to discover recent genres, 'help' to see available commands:");
//            if (args.Length > 0) player.ReadCmd(args);
//            while (player.working)
//            {
//                player.ReadCmd(p);
//            }
        }
    }
    public class DriverBass
    {
        int channel;
        public float vol = 100;
        public DriverBass() 
        {
            BassNet.Registration("tobij@gnail.pw", "2X18241914151432");
        }
        public bool Play(string url)
        {
            if (channel != 0) Bass.BASS_StreamFree(channel);
            else
            {
                Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            }
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_PLAYLIST, 2);
            try
            {
                channel = Bass.BASS_StreamCreateURL(url, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
            }
            catch
            {
                //Console.WriteLine("Stream error: " + Bass.BASS_ErrorGetCode());
                return false;
            }
            if (channel != 0)
                {
                    Volume(vol);
                    return Bass.BASS_ChannelPlay(channel, false);
                }
            else
                {
                    //Console.WriteLine("Stream error: " + Bass.BASS_ErrorGetCode());
                    return false;
                }
        }
        public bool Stop()
        {
            return Bass.BASS_ChannelStop(channel);
        }
        public bool Pause()
        {
            return Bass.BASS_ChannelPause(channel);
        }
        public bool Volume(float value)
        {
            vol = value;
            return Bass.BASS_ChannelSetAttribute(channel,BASSAttribute.BASS_ATTRIB_VOL,value/100);
        }
        public string Now() 
        {
            string[] b = Bass.BASS_ChannelGetTagsMETA(channel);
            if (b == null) return "Unknown";
            int sep = b[0].IndexOf(";");
            if (sep == 0) return "Unknown";
            string title = b[0].Substring(0,sep);
            title = title.Substring(title.IndexOf("=")+1).Replace("'","");
            if (title == "") return "Unknown";
            return title;
        }
        public bool isPaused() 
        {
            return Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_PAUSED;
        }
    }
    public class Player : INotifyPropertyChanged
    {
        private string status = "Nothing is playing";
        public string Status {
            get { return status; }
            set {
                if ( status != value ) {
                    status = value;
                    OnPropertyChanged( "Status" );
                }
            }
        }

        public const string VERSION = "0.1.3";
        public const string SERVER_URL = "http://dir-test.xiph.org/";
        public const string SHOUTCAST = "http://yp.shoutcast.com/sbin/";
        public Station now;
        // Exit flag
        public bool working = true;
        private bool shout = false;
        internal string[] cmd;
        private string previous;
        private string[] commongenres = { "Active rock", "Adult album alternative", "Adult contemporary music", "Adult standards / nostalgia", "Adult hits", "Album rock", "Alternative rock", "Americana", "Beautiful music", "Big band", "Bluegrass", "Blues", "Caribbean (reggae, soca, merengue, cumbia, salsa, etc.)", "Christian music", "istian rock", "temporary Christian", "Christmas music", "Classic hits", "Classic rock", "Classical", "Contemporary hit radio (CHR, top-40 / hot hits)", "Contemporary classical music", "Country", "Dance", "Easy Listening", "Eclectic", "Folk music", "Hispanic rhythmic", "Indian music", "Jazz", "Mainstream rock", "Middle of the road (MOR)", "Modern adult contemporary (Modern AC)", "Modern rock", "Oldies", "Polka", "Progressive rock", "Psychedelic rock", "Quiet Storm", "Ranchera", "Regional Mexican (Banda, corridos, ranchera, conjunto, mariachi, norteno, etc.)", "Rhythmic adult contemporary", "Rhythmic contemporary (Rhythmic Top 40)", "Rhythmic oldies", "Rock", "Rock en espanol", "Romantica (Spanish AC)", "Smooth jazz", "Soft adult contemporary (soft AC)", "Soft rock", "Soul music", "Space music", "Traditional pop music", "Tropical (salsa, merengue, cumbia, etc.)", "Urban", "Urban contemporary (mostly rap, hip hop, soul, and R&B artists)", "Urban adult contemporary (Urban AC) - R&B, soul and sometimes gospel music, without rap", "Variety", "World music" };
        private DriverBass driver;
        private SortedSet<string> commands = new SortedSet<string>(){ "p", "play", "pause", "genre", "skip", "next", "exit", "quit", "v", "volume", "version", "stop", "now", "info", "shoutcast", "icecast", "help", "/?", "" };
        public Player() {  driver = new DriverBass(); }
        public void ReadCmd(string[] args)
        {
            if (args.Length < 1)
            {
                char[] spchars = { ' ' };
                Console.Write("> ");
                string input = Console.ReadLine();
                cmd = input.Split(spchars);
            }
            else
            {
                cmd = args;
            }
            cmd[0] = ClosestMatch(cmd[0]);
            switch (cmd[0])
            {
                case "play": Play(); break;
                case "pause": case "p": if (driver.isPaused()) Play(); else Pause(); break;
                case "genre": case "g": Genre(); break;
                case "skip": case "next": Skip(); break;
                case "exit": case "quit": working = false; break;
                case "volume": case "v": Volume(); break;
                case "version": Console.WriteLine("CMDRADIO v" + VERSION + " by mitrich <mitrich.kh@gmail.com>"); break;
                case "stop": Stop(); break;
                case "now": case "info": Now(); break;
                case "shoutcast": Console.WriteLine("Stations source set to shoutcast.com"); shout = true; break;
                case "icecast": Console.WriteLine("Stations source set to dir.xiph.org"); shout = false; break;
                case "": break;
                case "help": case "/?": Usage(); break;
                default: Console.WriteLine("No such command. Type 'play <genrename>' to listen music, 'help' to see available commands "); break;
            }
        }
        private string ClosestMatch(string CommandInput)
        {
            // Look for perfect match
            IEnumerable<string> filtered = commands.Where(x => x.Equals(CommandInput));
            if (!filtered.Any())
            {
                // Failing that, look for closest match
                filtered = commands.Where(x => x.StartsWith(CommandInput));
            }
            if (filtered.Any())
            {
                if (filtered.Count()>1)
                {
                    Console.WriteLine("No unambiguous match found. Matching commands are:");
                    foreach (string partial in filtered)
                    {
                        Console.Write(partial + " ");
                    }
                    Console.WriteLine();
                    return "";
                }
                else 
                {
                    return filtered.First();
                }
            }
            return CommandInput;
        }
        private void Usage()
        {
            Console.WriteLine("USAGE: cmdradio [command]\n");
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  icecast|shoutcast      Set stations directory: Icecast(open) or SHOUTcast(bigger)");
            Console.WriteLine("  play [genre], p        Play genre radio and resume playback when paused");
            Console.WriteLine("  pause, p               Pause playback");
            Console.WriteLine("  stop, s                Stop playback");
            Console.WriteLine("  skip, next, n          Change radio");
            Console.WriteLine("  volume <integer>, v    See or set sound volume (0-100)");
            Console.WriteLine("  now                    Check what is playing now");
            Console.WriteLine("  genre [keyword], g     Show recent genres of music or search all with keyword");
            Console.WriteLine("  help                   This page");
            Console.WriteLine("  version                Print version number");
            Console.WriteLine("  exit, quit, q          Close program");
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine("  cmdradio play rock     To play rock radio");
        }
        public void Play()
        {
            if ((now != null) && (cmd.Length == 1))
            {
                driver.Play(now.listen_url[0]);
                return;
            }
            if (cmd.Length == 1)
            {
                cmd = new string[] { "play", "" };
            }
            previous = cmd[1];
            if (cmd[1].Contains("://"))
            {
                now = new Station();
                //Console.WriteLine("Playing URL");
                Status = "Playing URL";
                driver.Play(cmd[1]);
                return;
            }
            //Console.WriteLine("Looking for " + (cmd[1].Equals("") ? "random" : cmd[1]) + " stations ...");
            Status = "Looking for " + ( cmd[ 1 ].Equals( "" ) ? "random" : cmd[ 1 ] ) + " stations ...";
            //Console.SetCursorPosition(0, Console.CursorTop - 1);
            if (shout)
            {
                if (cmd[1] == "") cmd[1] = "random";
                string xml = HttpReq(SHOUTCAST + "newxml.phtml?search=" + cmd[1]);
                if (xml == "")
                {
                    //Console.WriteLine("Not found\n");
                    Status = "Not found";
                    return;
                }
                int needle = xml.IndexOf('\n') + 1;
                xml = xml.Insert(needle, "<root>\n") + "</root>";
                StringReader sreader = new StringReader(xml);
                XmlSerializer serializer = new XmlSerializer(typeof(stationlist));
                stationlist stations = (stationlist)serializer.Deserialize(sreader);
                sreader.Close();
                if (stations.stations.Length == 0)
                {
                    //Console.WriteLine("Not found\n");
                    Status = "Not found";
                    return;
                }
                Random random = new Random();
                station sh = stations.stations[random.Next(0, stations.stations.Length)];
                now = new Station();
                now.bitrate = int.Parse(sh.bitrate);
                now.genre = new String[] { sh.genre };
                now.listen_url = new String[] { SHOUTCAST + "tunein-station.pls?id=" + sh.id };
                now.server_type = sh.type;
                now.server_name = sh.name;
                now.current_song = sh.ct;
                now.channels = 0;
                now.samplerate = 0;
            }
            else
            {
                String json = HttpReq(SERVER_URL + "streams/?genre=" + cmd[1]);
                if (json == "") return;
                JsonSerializer ser = new JsonSerializer();
                Station sta = JsonConvert.DeserializeObject<StreamsResponse>(json).stations[0];
                now = sta;
            }
            //Console.WriteLine("Playing: " + now.server_name[0] + " [" + now.listen_url[0] + "]" + " <" + now.genre[0] + "> "+now.server_type[0]+" "+now.bitrate[0]+"kbit/s\n");
            Status = "Playing: " + now.server_name[0] + "\n[" + now.listen_url[0] + "]" + " <" + now.genre[0] + "> " + now.server_type[0] + "\n" + now.bitrate + "kbit/s\n";
            driver.Play(now.listen_url[0]);
            //Console.WriteLine("Current song: "+driver.Now());
        }
        private void Skip()
        {
            if (now == null) {
                Status = "Nothing is playing";
                //Console.WriteLine("Nothing is playing");
                return;
            }
            cmd = new string[] { "play", previous };
            Play();
        }

        public void Pause()
        {
            if (now == null)
            {
                //Console.WriteLine("Nothing is playing");
                return;
            }
            Status = "Nothing is playing";
            driver.Pause();
        }
        private void Stop()
        {
            if (now == null)
            {
                //Console.WriteLine("Nothing is playing");
                return;
            }
            Status = "Nothing is playing";
            driver.Stop();
            now = null;
        }
        private void Now()
        {
            if (now != null)
                Console.WriteLine("Playing: " + now.server_name[0] + " [" + now.listen_url[0] + "]" + " <" + now.genre[0] + "> " + now.server_type[0] + " " + now.bitrate + "kbit/s\n" + "Current song: " + driver.Now());
            else
                Console.WriteLine("Nothing is playing");
        }
        public List< String > GetGenres( string search = "" ) {
            List<string> keygenres = new List<string>();
            if (shout)
            {
                StringReader sreader = ShoutcastReq("http://yp.shoutcast.com/sbin/newxml.phtml");
                XmlSerializer serializer = new XmlSerializer(typeof(genrelist));
                genrelist genres = (genrelist)serializer.Deserialize(sreader);
                sreader.Close();
                foreach (genre g in genres.genre)
                {
                    keygenres.Add(g.name);
                }
            }
            else {
                string url = SERVER_URL + "genres/" + search;
                String json = HttpReq(url);
                if (json == "") return new List< string >();
                keygenres = JsonConvert.DeserializeObject<List<genre2>>(json).Select(t => t.name).ToList();
            }
            return keygenres;
        }
        private void Genre()
        {
            if (cmd.Length > 1)
            {
                List<string> keygenres = new List<string>();
                if (shout)
                {
                    StringReader sreader = ShoutcastReq("http://yp.shoutcast.com/sbin/newxml.phtml");
                    XmlSerializer serializer = new XmlSerializer(typeof(genrelist));
                    genrelist genres = (genrelist)serializer.Deserialize(sreader);
                    sreader.Close();
                    foreach (genre g in genres.genre)
                    {
                        keygenres.Add(g.name);
                    }
                }
                else
                {
                    string url = SERVER_URL + "genres/" + cmd[1];
                    String json = HttpReq(url);
                    if (json == "") return;
                    keygenres = JsonConvert.DeserializeObject<List<string>>(json);
                }
                Console.WriteLine(string.Join("\n", keygenres.ToArray()));
            }
            else
            {
                Console.WriteLine(string.Join("\n", commongenres));
            }
        }
        private void Volume()
        {
            int vol = 0;
            if (cmd.Length == 1)
            {
                //Console.WriteLine("Volume value: "+driver.vol);
                return;
            }
            try
            {
                vol = Convert.ToInt32(cmd[1]);
            }
            catch (FormatException e)
            {
                Console.WriteLine("Volume value is invalid.");
            }
            catch (OverflowException e)
            {
                Console.WriteLine("Volume value is invalid.");
            }
            finally
            {
                if ((vol <= 100) && (vol >= 0))
                {
                    //Console.WriteLine("Volume value changed to {0} %", vol);
                    driver.Volume(vol);
                }
                else
                {
                    //Console.WriteLine("Volume can be from 0 to 100.");
                }
            }
        }
        private StringReader ShoutcastReq(string uri) 
        {
            string xml = HttpReq(uri);
            if (xml == "") Console.WriteLine("Not found");
            int needle = xml.IndexOf('\n')+1;
            xml = xml.Insert(needle, "<root>\n") + "</root>";
            StringReader sreader = new StringReader(xml);
            return sreader;
        }
        private string HttpReq(string uri)
        {
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.66 Safari/537.36");
            client.Headers.Add("Accept", "*/*");
            //client.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            client.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            client.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            Stream data;
            try
            {
                data = client.OpenRead(uri);
            }
            catch (WebException e)
            {
                //Console.WriteLine("Cannot find request data                   ");
                return "";
            }
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            return s;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged( string propertyName ) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    public class StreamsResponse {
        [JsonProperty("streams")]
        public List<Station> stations;
    }
    
    public class Station
    {
        [JsonProperty("stream_name")]
        public String server_name;
        [JsonProperty("listenurls")]
        public String[] listen_url;
        [JsonProperty("stream_type")]
        public String server_type;
        public int? bitrate;
        public int? channels;
        public int? samplerate;
        [JsonProperty("genres")]
        public String[] genre;
        [JsonProperty("songname")]
        public String current_song;
        [JsonProperty("id")]
        public String _id;
    }
    [Serializable]
    public class genre
    {
        [XmlAttribute("name")]
        public string name;
    }


    [Serializable]
    [XmlRoot("root")]
    public class genrelist
    {
        [XmlArray("genrelist")]
        [XmlArrayItem("genre", typeof(genre))]
        public genre[] genre;
    }
    [Serializable]
    [XmlRoot("root")]
    public class stationlist
    {
        [XmlArray("stationlist")]
        public station[] stations;
    }
    [Serializable]
    public class station 
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("mt")]
        public string type;
        [XmlAttribute("id")]
        public string id;
        [XmlAttribute("br")]
        public string bitrate;
        [XmlAttribute("genre")]
        public string genre;
        [XmlAttribute("ct")]
        public string ct;
        [XmlAttribute("lc")]
        public string lc;
    }

    [JsonObject]
    public class genre2 {
        [JsonProperty("val")]
        public string name;
    }
}
