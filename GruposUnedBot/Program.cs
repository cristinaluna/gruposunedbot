using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace GruposUnedBot
{
    class Program
    {
        static string ApplicationName = "GruposUnedBot";
        static Telegram.Bot.ITelegramBotClient bot;
        static Random rng = new Random();
        static object m_mutex = new object();
        static Config config = new Config("config/config.xml", "config/token.txt");
        static void Main(string[] args)
        {
            string telegramToken = config.Token;
            bool initialized = false;
            bool subscribed = false;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            while (!initialized)
            {
                if (bot != null && subscribed)
                {
                    bot.OnCallbackQuery -= BotOnCallbackQueryReceived;
                    bot.OnMessage -= Bot_OnMessage;
                    bot.OnMessageEdited -= Bot_OnMessage;
                    bot.OnInlineQuery -= BotOnInlineQueryReceived;
                    bot.OnInlineResultChosen -= BotOnChosenInlineResultReceived;
                    bot.OnReceiveError -= BotOnReceiveError;
                    subscribed = false;
                }

                bot = new Telegram.Bot.TelegramBotClient(telegramToken);
                bot.OnCallbackQuery += BotOnCallbackQueryReceived;
                bot.OnMessage += Bot_OnMessage;
                bot.OnMessageEdited += Bot_OnMessage;
                bot.OnInlineQuery += BotOnInlineQueryReceived;
                bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
                bot.OnReceiveError += BotOnReceiveError;
                subscribed = true;

                var me = bot.GetMeAsync();
                bot.StartReceiving();

                while (me.Status == TaskStatus.WaitingForActivation || me.Status == TaskStatus.Created || me.Status == TaskStatus.Running)
                    System.Threading.Thread.Sleep(100);

                if (me.Status == TaskStatus.RanToCompletion)
                {
                    initialized = true;
                }
                else
                {
                    Console.WriteLine("Bot status: " + me.Status + ". Retrying...");
                    System.Threading.Thread.Sleep(60000);
                }
            }
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            long counter = 0;
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                ++counter;

                // Each quarter hour
                
            }
        }

        private static void SendSticker(string stickerName, Telegram.Bot.Types.Message msg)
        {
            if (stickerName != null && stickerName.Length > 0)
            {
                string sticker = $"stickers/{stickerName}.webp";
                Console.WriteLine($"Mostrando sticker {sticker}");
                if (File.Exists(sticker))
                {
                    using (FileStream fs = new FileStream(sticker, FileMode.Open))
                    {
                        Task<Telegram.Bot.Types.Message> task;
                        if (msg is null)
                            task = bot.SendStickerAsync(-1001277182031, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs),
                                disableNotification: true);
                        else
                            task = bot.SendStickerAsync(msg.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs),
                                replyToMessageId: msg.MessageId, disableNotification: true);

                        task.Wait();
                        var result = task.Result;
                    }
                }
            }
        }

        private static void SendTextMessage(string text, Telegram.Bot.Types.Message msg)
        {
            if (text != null && text.Length > 0)
            {
                Console.WriteLine($"Mostrando texto {text}");
                Task<Telegram.Bot.Types.Message> task;
                if (msg is null)
                    task = bot.SendTextMessageAsync(-1001277182031, text, disableNotification: true);
                else
                {

                    try
                    {
                        bot.SendTextMessageAsync(msg.Chat.Id, text,
                            replyToMessageId: msg.MessageId, disableNotification: true);
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        private static void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            Console.WriteLine(e.ApiRequestException.ToString());
        }

        private static void BotOnNewUsers(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }


        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            lock (m_mutex)
            {
                string reply = "";
                string replyNoQuote = "";
                string replySticker = "";
                string replyFile = "";
                string replyGif = "";
                string replyMail = "";
                string replyVideo = "";
                string replyDir = "";
                string verification = "";
                string path2Verification = "config/validatedusers.csv";
                string user2Verify = "";

                HashSet<String> wordsSet = new HashSet<string>();

                var msg = e.Message;

                if (msg == null || msg.Text == null && msg.Caption == null)
                    return;
                else if (msg == null || msg.Text == null && msg.Caption != null)
                    msg.Text = msg.Caption;

                Console.WriteLine($"Mensaje de {msg.From.FirstName} {msg.From.LastName} ({msg.From.Username}, id {msg.From.Id}: {msg.Text}");

                string msgBody = msg.Text.ToLowerInvariant();
                string username = msg.From.Username;
                if (username == null || username.Length == 0)
                {
                    username = msg.From.FirstName + " " + msg.From.LastName;
                }
                if (msgBody != null)
                {
                    msgBody = msgBody.ReplaceAny("\\\"´¨`^|·#~%¬&()[]{}<>=¿?¡!^+´¨;-÷".ToCharArray(), " ");
                    msgBody = msgBody.ReplaceAny("\n\t\r".ToCharArray(), " ");
                    //msgBody = msgBody.ReplaceAny("@*".ToCharArray(), "");
                    msgBody = msgBody.ReplaceAny("áäàâãª".ToCharArray(), "a");
                    msgBody = msgBody.ReplaceAny("éëèê€".ToCharArray(), "e");
                    msgBody = msgBody.ReplaceAny("íïìî".ToCharArray(), "i");
                    msgBody = msgBody.ReplaceAny("óöòôõº".ToCharArray(), "o");
                    msgBody = msgBody.ReplaceAny("úüùû".ToCharArray(), "u");
                    msgBody = msgBody.ReplaceAny("¥".ToCharArray(), "y");
                    msgBody = msgBody.ReplaceAny("£".ToCharArray(), "l");
                    msgBody = msgBody.ReplaceAny("$".ToCharArray(), "s");
                    msgBody = msgBody.ReplaceAny("".ToCharArray(), "e");
                    msgBody = msgBody.ReplaceAny("çÇ".ToCharArray(), "c");

                    msgBody = msgBody.Trim();

                    foreach (var word in msgBody.Split(" \n\r\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        var notnum = word.ToList().Where(c => !("0123456789.,'".ToList().Contains(c)));
                        if (notnum.Count() == 0 || notnum.Count() == 1 && word.Length > 1 && word.EndsWith("k"))
                            wordsSet.Add(word);
                    }

                    
                    Console.WriteLine($"Mensaje procesado: {msgBody}");
                    int lastWordPos = msgBody.LastIndexOf(" ") + 1;



                    string lastWord = msgBody.Substring(lastWordPos);
                    if (lastWord.Length > 0)
                    {

                        if (!msgBody.Contains("public poll") && !msgBody.Contains("anonymous poll"))
                        {

                            //Aquí las que no son trollines
                            else if (msgBody.Contains("/start") && !msgBody.Contains("werewolf"))
                            {
                                reply = $"Para validarte en el sistema, envíame tu dirección de correo de la UNED.";
                            }
                            else if (msgBody.Contains("@alumno.uned.es"))
                            {
                                replyMail = msgBody.ToString();
                                user2Verify = $"{username}";
                            }
                            else if (msgBody.Contains("@uned.es"))
                            {
                                reply = $"APOCALIPSIS ALERT!\nEl mail que has introducido no es un correo válido de alumnx.";
                            }
                            else if (msgBody.Contains("code:"))
                                reply = $"verify";
                            else if (msgBody.Contains("direccion") && msgBody.Contains("facultad"))
                                reply = $"facultad";
                            else if ((msgBody.Contains("direccion") || msgBody.Contains("telef")) && msgBody.Contains("centro"))
                            {
                                replyDir = "dir";
                            }
                            else if (msgBody.Contains("verificar") && msgBody.Contains("@"))
                                reply = $"verificar";
                            else if (msgBody.Contains("telef") && msgBody.Contains("facultad"))
                                reply = $"telefonos";
                            else if (msgBody.Contains("direccion") && msgBody.Contains("facultad"))
                                reply = $"facultad";
                            else if (msgBody.Contains("ayuda") && msgBody.Contains("bot"))
                                reply = $"¿Necesitas ayuda? \n Soy un bot etnográfico de @GruposUneduned, estoy aquí para observar y no intervenir.\n" +
                                    $"Prueba a pedir la dirección de la facultad, los telefonos o el listado de enlaces.\n" +
                                    $"\nSi quieres saber más de mí, puedes preguntarle a mi creadora.";
                            else if (msgBody.Contains("faq"))
                                reply = $"ALGUNAS DE LAS FAQ \n " +
                                    $"## ¿Qué hace el bot? ##\n Si me pides ayuda te indicaré algunas funciones. Estoy aquí para ayudar a les administradores del grupo y a sus miembres, " +
                                    $"estoy muy bien informade y quiero ayudar.\n\n" +
                                    $"## ¿Cuándo empieza el curso o cuándo son los exámenes? ##\n Puedes pedirme el calendario académico si lo necesitas.\n\n" +
                                    $"## ¿Cuál es el precio de matrícula? ##\n Puedes pedirme también los precios públicos si los necesitas.\n\n" +
                                    $"## ¿De cuántas asignaturas me debería matricular? ##\n Eso depende de cada une, piensa que un cuatrimestre entero está pensado" +
                                    $" para dedicarle 8h al día, por lo que medio curso para 4h diarias. Cada ECTS equivale a 25h de estudio. El número de créditos " +
                                    $"en los que te puedas matricular dependerá de tu dedicación.\n\n" +
                                    $"## Háblame de las CONVALIDACIONES ## \n Puedes pedirme la tabla de reconocimiento de créditos para ver qué podrían convalidarte." +
                                    $"También puedes pedirme el formulario de solicitud de reconocimiento de créditos o la propia normativa.\n\n" +
                                    $"## TFG ## \n Tengo también la normativa del tfg y de matriculación de este. \n\n" +
                                    $"## ¿Y las becas? ##\n Pídeme información sobre las becas.\n\n" +
                                    $"## ¿Cómo validarme como alumnx? ##\n Para la verificación tienes que enviarme tu correo\n" +
                                    $" @alumno.uned.es por privado y seguir los pasos. Si te validaste y has cambiado de alias" +
                                    $" es probable que tengas que volverte a validar.\n\n" +
                                    $"## ¿Por qué es mejor que ponga un alias en telegram? ##\n Con el alias podemos escribirte sin necesidad de que compartas" +
                                    $" tu tlf. Y para mí también será más fácil saber guardar tu verificación. Si no sabes cómo poner un alias, puedes preguntarme " +
                                    $"cómo poner un alias.\n\n" +
                                    $"## Direcciones de contacto ##\n" +
                                    $"Puedes pedirme la dirección de la facultad, o la dirección del centro asociado de la provincia " +
                                    $"que necesites.\n\n";
                            else if (msgBody.Contains("listado") && msgBody.Contains("enlaces"))
                                reply = $"listen";
                            else if (msgBody.Contains("como") && msgBody.Contains("alias"))
                                reply = $"Aquí tienes un tutorial de cómo hacerlo: \n " +
                                    $"https://bit.ly/2G0YxYM";
                            else if ((msgBody.Contains("calendario") && msgBody.Contains("academico"))
                                || ((msgBody.Contains("abre") || msgBody.Contains("apertura")) && msgBody.Contains("cursos"))
                                || ((msgBody.Contains("abre") || msgBody.Contains("apertura")) && msgBody.Contains("alf")))
                                replyFile = $"http://portal.uned.es/pls/portal/docs/PAGE/UNED_MAIN/INFORMACIONGENERAL/NUEVO%20PORLET%20COORDINACI%C3%93N%20INFORMATIVA/CAL-ACAD-ADM-20-21.PDF";
                            else if (msgBody.Contains("precios") && msgBody.Contains("publicos"))
                                replyFile = $"http://portal.uned.es/pls/portal/docs/PAGE/UNED_MAIN/INFORMACIONGENERAL/PDF'S/BOE-A-2020-7705.PDF";
                            else if (msgBody.Contains("normativa") && msgBody.Contains("matriculacion tfg"))
                                replyFile = $"https://descargas.uned.es/publico/pdf/guias/grados/normativa/NORMATIVA_MATRICULACION_TFG_CUATRIMESTRALES.pdf";
                            else if (msgBody.Contains("normativa") && msgBody.Contains("tfg"))
                                replyFile = $"https://descargas.uned.es/publico/pdf/guias/grados/normativa/NORMATIVA_TFG_04242018.pdf";
                            else if (msgBody.Contains("normativa") && msgBody.Contains("permanencia"))
                                replyFile = $"https://descargas.uned.es/publico/pdf/guias/grados/normativa/NORMAS_PERMANENCIA_CG-06102015.pdf";
                            else if (msgBody.Contains("normativa") && msgBody.Contains("creditos"))
                                replyFile = $"http://portal.uned.es/pls/portal/docs/PAGE/UNED_MAIN/LAUNIVERSIDAD/UBICACIONES/10/ADMINISTRACION/TRAMITESADMIN/CONVALIDACIONES/1.-NORMATIVA%20RC-23.10.2008%20MODIF.%2028.06.2011%20Y%2004.10.2016%20(ACTUALIZACIONES)%202019_04_05.PDF";
                            else if ((msgBody.Contains("solicitud") || msgBody.Contains("formulario")) && msgBody.Contains("creditos") && msgBody.Contains("reconocimiento"))
                                replyFile = $"http://portal.uned.es/pls/portal/docs/PAGE/UNED_MAIN/LAUNIVERSIDAD/UBICACIONES/10/ADMINISTRACION/TRAMITESADMIN/CONVALIDACIONES/SOLICITUD%20RECONOCIMIENTO%20GRADO.PDF";
                            else if (msgBody.Contains("tabla") && msgBody.Contains("reconocimiento") && msgBody.Contains("creditos"))
                                replyFile = $"http://portal.uned.es/pls/portal/docs/PAGE/UNED_MAIN/LAUNIVERSIDAD/UBICACIONES/10/ADMINISTRACION/TRAMITESADMIN/CONVALIDACIONES/TABLA%20DE%20EQUIVALENCIA%20DE%20OTROS%20ESTUDIOS%20UNED%20AL%20GRADO%20DE%20GruposUnedLOGIA%20(1-7-19).PDF";
                            else if ((msgBody.Contains("guia") && msgBody.Contains("enlace"))
                                || (msgBody.Contains("pasa") && msgBody.Contains("guia")))
                            {

                                if (msgBody.Contains("primero") || msgBody.Contains("1"))
                                    reply = $"guias1";
                                else if (msgBody.Contains("segundo") || msgBody.Contains("2"))
                                    reply = $"guias2";
                                else if (msgBody.Contains("tercero") || msgBody.Contains("3"))
                                    reply = $"guias3";
                                else if (msgBody.Contains("cuarto") || msgBody.Contains("4"))
                                    reply = $"guias4";
                                else
                                    reply = $"Prueba a pedirme un enlace para las guías del curso que quieras";
                            }
                            else if (msgBody.Contains("enlace") && msgBody.Contains("estg"))
                                reply = $"enlace_estg";
                            else if (msgBody.Contains("enlaces") && msgBody.Contains("grupos"))
                                reply = $"enlaces_grupos";

                            //y ahora el resto
                            else if (msgBody.Length > 300)
                                reply = $"mucho texto.";
                            else if (msgBody.Contains("seguro dental"))
                                reply = $"LISA NECESITA UN APARATO\n";
                            else if (msgBody.Contains("bot") && msgBody.Contains("años tienes"))
                                reply = $"Se podría decir que nací ayer\n";
                            else if (msgBody.Contains("bot") && msgBody.Contains("quien eres"))
                                reply = $"Soy una ayudita a lxs admins de este grupo para poder controlaros.\n";
                            else if (msgBody.Contains("bot") && msgBody.Contains("hola"))
                                reply = $"Hola,  @{username}\n";
                            else if (msgBody.Contains("bot") && msgBody.Contains("te amo"))
                                reply = $"Necesito más tiempo, @{username}.\n";
                            else if (msgBody.Contains("bot") && msgBody.Contains("te quiero"))
                            {
                                int random = rng.Next(1, 6 + 1);
                                if (random == 1)
                                    reply = $"A mí no me caes ni bien, @{username}";
                                else if (random == 2)
                                    replyGif = "https://media.giphy.com/media/l0NgR62Ooi7ftNlHq/giphy.gif";
                                else if (random == 3)
                                    reply = $"Podemos ser amiguis, @{username}";
                                else if (random == 4)
                                    reply = $"¿Sabes esas historias románticas donde dos personas se odian y al final se aman, @{username}? " +
                                        $"Pues esto igual pero sin el final.";
                                else if (random == 5)
                                    replyGif = "https://media.giphy.com/media/PQfNP9zAdMgWTRjPc6/giphy.gif";
                                else if (random == 6)
                                    replyGif = "https://media.giphy.com/media/MA8BOGCdGy2LC/giphy.gif";
                            }
                            else if (msgBody.Contains("bot") && msgBody.Contains("eres malo"))
                                reply = $"No me consta.\n";
                            else if (msgBody.Contains("hazme") && msgBody.Contains("caso"))
                                reply = $"No me seas brasas, @{username}.\n";
                            else if (msgBody.Contains("bot") && msgBody.Contains("te odio"))
                                reply = $"Suuuu cómo me renta un buen odiar, @{username}\n";
                            else if (msgBody.Contains("diosa") || msgBody.Contains("creador") || msgBody.Contains("creacionista"))
                                reply = $"Mi diosa y creadora es: @mascroquetas\n";
                            else if (msgBody.Contains("buenos dias"))
                                reply = $"Buenos días, @{username}";
                            else if (lastWord == "noice")
                                replyGif = "https://media.giphy.com/media/yJFeycRK2DB4c/giphy.gif";
                            else if (msgBody.Equals("jo"))
                                reply = $"Menos lamentos, @{username}.\n";
                            else if (msgBody.Contains("suuuuuuuu"))
                            { 
                                int random = rng.Next(1, 6 + 1);
                                if (random == 1)
                                    reply = "JODEEEEEEEER QUE SUUUUUUUUUUUUUUUUUU.";
                                else if (random == 2)
                                    replyGif = "https://media.giphy.com/media/eMu0803X2zkWY/giphy.gif";
                                else if (random == 3)
                                    replyGif = "https://media.giphy.com/media/F9hQLAVhWnL56/giphy.gif.";
                                else if (random == 4)
                                    replyGif = "https://media.giphy.com/media/wu7cpUdNkAdY4/giphy.gif";
                            }   
                            else if (msgBody.Contains("jjj"))
                                reply = $"¿Te ha dado un ictus o te has atragantado? xddd\n";
                            else if (msgBody.Contains("nazi"))
                                replyGif = "https://tenor.com/es/ver/family-guy-peter-griffin-nazi-stuff-gif-13917915";
                            else if (msgBody.Contains("niñ") && msgBody.Contains("rata"))
                                replyGif = "https://media.giphy.com/media/3orieNrNZWfqCkG8vK/giphy.gif";
                            else if (msgBody.Contains("oye") && msgBody.Contains("bot"))
                                reply = $"Te leo, @{username}.\n";
                            else if (msgBody.Contains("bot") && msgBody.Contains("opinas de"))
                                reply = $"Para poder opinar necesitaría realizar un estudio completo. Sin intervenir, claro, porque soy un bot. De momento dejaré mi opinión vacía, " +
                                    $"como mi corazón, hasta que pueda llenarla con conocimiento.\n";
                            else if (msgBody.Contains("te") && msgBody.Contains("dije"))
                                reply = $"Ti diji mimimimimi\n";
                            

                        }


                    }
                }


                if (reply == "" && replyNoQuote == "" && msg.Text.Trim().EndsWith("?"))
                {
                    int random = rng.Next(1, 10 + 1);
                    if (random == 1)
                        reply = "Obvio";
                    else if (random == 2)
                        reply = "Pa qué quieres saber eso haha salu2";
                    else if (random == 3)
                        reply = "No me consta.";
                    else if (random == 4)
                        reply = "Presuntamente.";
                    else if (random == 5)
                        replyGif = "https://tenor.com/MJUR.gif";
                }
                if (replyNoQuote.Length > 0)
                    bot.SendTextMessageAsync(msg.Chat.Id, replyNoQuote, disableNotification: true);
                else if (reply.Contains("verify"))
                {
                    string verCode = "";
                    var splits = msgBody.ToString().Split(':');
                    verCode = splits[1];
                    if(verifyUser(verCode, username, path2Verification))
                        bot.SendTextMessageAsync(msg.Chat.Id, text: "Verificación correcta.");
                    else
                        bot.SendTextMessageAsync(msg.Chat.Id, text: "UPS algo ha fallado en la verificación.");
                }
                else if (reply.Contains("verificar"))
                {
                    string user = "";
                    string replyVer = "";
                    var splits = msgBody.ToString().Split('@');
                    user = splits[1];
                    if (isVerified(user, path2Verification))
                    {
                        replyVer = String.Concat("@", user, " es alumnx de la uned.");
                    }
                    else
                    {
                        replyVer = String.Concat("@", user, " NO se ha verificado como alumnx de la uned.");
                    }
                    bot.SendTextMessageAsync(msg.Chat.Id, replyVer, replyToMessageId: msg.MessageId, disableNotification: true);
                }
                else if (reply.Contains("facultad filosofia") && !reply.Contains("ayuda"))
                {
                    bot.SendContactAsync(msg.Chat.Id, phoneNumber: "+34913989373", firstName: "Negociado de Estudiantes");
                    bot.SendContactAsync(msg.Chat.Id, phoneNumber: "+34913986818", firstName: "Convalidaciones");
                    bot.SendContactAsync(msg.Chat.Id, phoneNumber: "+34913988955", firstName: "Atención al Estudiante");
                    bot.SendVenueAsync(msg.Chat.Id, latitude: 40.436381f, longitude: -3.735008f, title: "Facultad de Filosofía UNED",
                        address: "Edificio de Humanidades\n C/Paseo Senda del Rey, 7 28040 - Madrid(España)");
                }
                else if (reply.Contains("telefonos filosofia") && !reply.Contains("ayuda"))
                {
                    bot.SendContactAsync(msg.Chat.Id, phoneNumber: "+34913989373", firstName: "Negociado de Estudiantes");
                    bot.SendContactAsync(msg.Chat.Id, phoneNumber: "+34913986818", firstName: "Convalidaciones");
                    bot.SendContactAsync(msg.Chat.Id, phoneNumber: "+34913988955", firstName: "Atención al Estudiante");
                }
  
                else if (reply.Contains("enlaces_grupos"))
                {
                    string replyESTG = "";
                    string enlace = "";
                    string replyEnlace = "";

                    if (isVerified(username, path2Verification))
                    {
                        replyESTG = String.Concat("@", username, ", te he enviado un privi");

                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {

                            
                            // first row
                            new []
                            {
                                InlineKeyboardButton.WithUrl("Grupo General", "https://t.me/joinchat/EzUxdUw1UtaN5g8iADMloQ"),
                                InlineKeyboardButton.WithUrl("Canal", "https://t.me/GruposUnedlogiauned"),
                            },
                            // first row
                            new []
                            {
                                InlineKeyboardButton.WithUrl("Grupo General", "https://t.me/joinchat/EzUxdUw1UtaN5g8iADMloQ"),
                                InlineKeyboardButton.WithUrl("Cognitiva y Simbólica I y II", "https://t.me/joinchat/EFTWIxrISJNt0HPAYrvR_g"),
                            },
                            // second row

                        
                            new []
                            {
                                InlineKeyboardButton.WithUrl("Horarios tutorías", "https://akademosweb.uned.es/Default.aspx"),
                                InlineKeyboardButton.WithUrl("Consulta calificaciones", "https://app.uned.es/gesmatri/Presentacion/modCalificaciones/listadoCalificaciones.aspx"),
                            },

                            new []
                            {
                                InlineKeyboardButton.WithUrl("Librería UNED", "https://www.librosuned.com/matriculauned/compras/cestaClavesUNED.aspx"),
                                InlineKeyboardButton.WithUrl("Verificación", "https://t.me/GruposUnedunedbot"),
                            }

                        });

                        replyEnlace = String.Concat("@", username, ", aquí tienes los enlaces: ", enlace);
                        bot.SendTextMessageAsync(msg.Chat.Id, replyESTG, replyToMessageId: msg.MessageId, disableNotification: true);
                        bot.SendTextMessageAsync(msg.From.Id, replyEnlace, replyMarkup: inlineKeyboard);
                    }
                    else
                    {
                        replyESTG = String.Concat("@", username, ", para enviarte el enlace debes verificarte como alumnx de la UNED. \n" +
                            "Háblame por privado y mándame tu correo de la UNED para ello.");
                        bot.SendTextMessageAsync(msg.Chat.Id, replyESTG, replyToMessageId: msg.MessageId, disableNotification: true);
                    }
                }
                else if (reply.Contains("listen"))
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithUrl("Grupo General", ""),
                            InlineKeyboardButton.WithUrl("Canal", ""),
                        },
                        // second row

                        
                        new []
                        {
                            InlineKeyboardButton.WithUrl("Horarios tutorías", "https://akademosweb.uned.es/Default.aspx"),
                            InlineKeyboardButton.WithUrl("Consulta calificaciones", "https://app.uned.es/gesmatri/Presentacion/modCalificaciones/listadoCalificaciones.aspx"),
                        },

                        new []
                        {
                            InlineKeyboardButton.WithUrl("Librería UNED", "https://www.librosuned.com/matriculauned/compras/cestaClavesUNED.aspx"),
                            InlineKeyboardButton.WithUrl("Verificación", "https://t.me/GruposUnedbot"),
                        }
                        
                    });
                    bot.SendTextMessageAsync(msg.Chat.Id, text: "Enlaces Grupos UNED", replyMarkup: inlineKeyboard);
                }
                else if (reply.Length > 0)
                    bot.SendTextMessageAsync(msg.Chat.Id, reply, replyToMessageId: msg.MessageId, disableNotification: true);
                else if (replySticker != null && replySticker.Length > 0)
                {
                    string sticker = $"stickers/{replySticker}.webp";
                    Console.WriteLine($"Mostrando sticker {sticker}");
                    if (File.Exists(sticker))
                    {
                        using (FileStream fs = new FileStream(sticker, FileMode.Open))
                        {
                            Task<Telegram.Bot.Types.Message> task = bot.SendStickerAsync(msg.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs),
                                replyToMessageId: msg.MessageId, disableNotification: true);
                            try
                            {
                                task.Wait();
                                var result = task.Result;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
                else if (replySticker != null && replySticker.Length > 0)
                {
                    string sticker = $"stickers/{replySticker}.webp";
                    Console.WriteLine($"Mostrando sticker {sticker}");
                    if (File.Exists(sticker))
                    {
                        using (FileStream fs = new FileStream(sticker, FileMode.Open))
                        {
                            Task<Telegram.Bot.Types.Message> task = bot.SendStickerAsync(msg.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs),
                                replyToMessageId: msg.MessageId, disableNotification: true);
                            try
                            {
                                task.Wait();
                                var result = task.Result;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
                else if (replyGif != null && replyGif.Length > 0)
                {
                    Task<Telegram.Bot.Types.Message> task = bot.SendVideoAsync(msg.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(replyGif),
                            replyToMessageId: msg.MessageId, disableNotification: true);
                    task.Wait();
                    var result = task.Result;
                }
                else if (replyVideo != null && replyVideo.Length > 0)
                {
                    Task<Telegram.Bot.Types.Message> task = bot.SendVideoAsync(msg.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(replyGif),
                            replyToMessageId: msg.MessageId, disableNotification: true);
                    task.Wait();
                    var result = task.Result;
                }
                else if (replyFile != null && replyFile.Length > 0)
                {
                    Task<Telegram.Bot.Types.Message> task = bot.SendDocumentAsync(msg.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(replyFile),
                            replyToMessageId: msg.MessageId, disableNotification: true);
                    task.Wait();
                    var result = task.Result;
                }
                else if (replyMail != null && replyMail.Length > 0)
                {
                    // This creates the file if doesn't exist

                    if (!File.Exists("config/loginmail.txt"))
                    {
                        Console.WriteLine("\nTienes que añadir el archivo con el login del mail config/loginmail.txt separando user y pass con una ,");

                        Task<Telegram.Bot.Types.Message> task = bot.SendTextMessageAsync(msg.Chat.Id, text: "No se ha podido enviar el correo.");
                        task.Wait();
                        var result = task.Result;
                    }
                    else
                    {
                        if (!File.Exists(path2Verification))
                        {
                            // Create a file to write to.
                            using (StreamWriter sw = File.CreateText(path2Verification))
                            {
                                sw.WriteLine("user,mail,code,validated,");
                            }
                        }
                        if (!findVertificatedUser(path2Verification, user2Verify))
                        {
                            verification = GetVerificationCode(5);
                            string verLine = String.Concat(user2Verify, ",", msgBody.ToString(), ",", verification, ",", "N");

                            int line = findUser(path2Verification, user2Verify);
                            if (line > 0)
                            {
                                lineChanger(verLine, path2Verification, line);
                            }
                            else
                            {
                                // This text is always added, making the file longer over time
                                // if it is not deleted.
                                using (StreamWriter sw = File.AppendText(path2Verification))
                                {
                                    sw.WriteLine(verLine);
                                }
                            }
                            sendMail(msgBody.ToString(), verification);
                            Task<Telegram.Bot.Types.Message> task = bot.SendTextMessageAsync(msg.Chat.Id, text: "Te he enviado un correo con el código de verificación. Recuerda que el formato de código que me tienes que enviar es: \"code:codig\"");
                            task.Wait();
                            var result = task.Result;
                        }
                        else
                        {
                            Task<Telegram.Bot.Types.Message> task = bot.SendTextMessageAsync(msg.Chat.Id, text: "Ya estás verificadx como estudiante de la UNED.");
                            task.Wait();
                            var result = task.Result;

                        }
                    }
                    
                }
                else if (replyDir != null && replyDir.Length > 0)
                {
                    if (!File.Exists("config/listadocentrosuned.csv"))
                    {
                        // print no direcciones disponibles
                        bot.SendTextMessageAsync(msg.Chat.Id, "No se encuentra el archivo de direcciones", replyToMessageId: msg.MessageId, disableNotification: true);
                    }
                    else
                    {
                        string centro = "";
                        string direccion = "";
                        string ciudad = "";
                        string tlf = "";
                        string provincia = "";
                        string email = "";
                        float latitud = 0f;
                        float longitud = 0f;
                        bool encontrado = false;
                        using (StreamReader sr = File.OpenText("config/listadocentrosuned.csv"))
                        {
                            string s = "";
                            while ((s = sr.ReadLine()) != null)
                            {
                                var splits = s.Split(';');
                                centro = replaceAccents(splits[0]);
                                direccion = replaceAccents(splits[1]);
                                ciudad = replaceAccents(splits[2]);
                                provincia = replaceAccents(splits[3]);
                                tlf = splits[4];
                                email = splits[5];
                                float.TryParse(splits[6], out latitud); Console.WriteLine(centro);
                                float.TryParse(splits[7], out longitud);
                                if (msgBody.Contains(centro.ToLower()) || msgBody.Contains(ciudad.ToLower()) || msgBody.Contains(provincia.ToLower()))
                                {
                                    bot.SendVenueAsync(msg.Chat.Id, latitude: latitud, longitude: longitud, title: string.Concat("Centro Asociado UNED ",centro),
                                        address: string.Concat(direccion));
                                    bot.SendContactAsync(msg.Chat.Id, phoneNumber: $"+34{tlf}", firstName: "UNED", lastName: centro,
                                        vCard: "BEGIN:VCARD\n" +
                                        "VERSION:3.0\n" +
                                        $"FN:CA UNED {centro}\n" +
                                        $"TEL;TYPE=work,voice;VALUE=uri:tel:+34{tlf}" +
                                        $"EMAIL:{email}\n" +
                                        "END:VCARD");
                                    bot.SendContactAsync(msg.Chat.Id, phoneNumber: $"+34{tlf}", firstName: "UNED", lastName: centro);
                                    encontrado = true;
                                }
                            }
                            if (!encontrado)
                            {
                                bot.SendTextMessageAsync(msg.Chat.Id, "No se ha encontrado el centro asociado.", replyToMessageId: msg.MessageId, disableNotification: true);
                            }
                        }
                    }
                }
            }

        }


        static string replaceAccents(string str)
        {
            str = str.ReplaceAny("áäàâãª".ToCharArray(), "a");
            str = str.ReplaceAny("éëèê€".ToCharArray(), "e");
            str = str.ReplaceAny("íïìî".ToCharArray(), "i");
            str = str.ReplaceAny("óöòôõº".ToCharArray(), "o");
            str = str.ReplaceAny("úüùû".ToCharArray(), "u");

            return str;
        }

        static string GetVerificationCode(int length)
        {
            char[] chArray = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string str = string.Empty;
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(1, chArray.Length);
                if (!str.Contains(chArray.GetValue(index).ToString()))
                {
                    str = str + chArray.GetValue(index);
                }
                else
                {
                    i--;
                }
            }
            return str;
        }

        static bool verifyUser(string verCode, string verUser, string path)
        {
            int line = 0;
            bool verified = false;
            string user = "";
            string mail = "";
            string code = "";
            string verifiedStr = "";
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    var splits = s.Split(',');
                    user = splits[0];
                    mail = splits[1];
                    code = splits[2];
                    if (verCode.ToLower().Equals(code.ToLower()) && verUser.ToLower().Equals(user.ToLower()))
                    {
                        verifiedStr = String.Concat(user, ",", mail, ",", verCode, ",", "Y");
                        Console.WriteLine("El user es " + user + " y el mail es " + mail);
                        verified = true;
                        break;
                    }
                    line++;
                }
            }
            if (verified)
                lineChanger(verifiedStr, path, line);
            return verified;
        }

        static bool isVerified(string user, string path)
        {
            bool verified = false;
            string userV = "";
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    var splits = s.Split(',');
                    userV = splits[0];
                    if (userV.ToLower().Equals(user.ToLower()) && splits[3].Equals("Y"))
                    {
                        verified = true;
                        break;
                    }
                }
            }
            return verified;
        }

        static void lineChanger(string newText, string path, int line2Edit)
        {
            string[] arrLine = File.ReadAllLines(path);
            arrLine[line2Edit] = newText;
            File.WriteAllLines(path, arrLine);
        }

        static bool findVertificatedUser(string path, string user)
        {
            bool returnVer = false;
            string verUser = "";
            string verificated = "";
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    var splits = s.Split(',');
                    verUser = splits[0];
                    verificated = splits[3];
                    if (verUser.Equals(user) && verificated.Equals("Y"))
                        returnVer = true;
                }
            }
            return returnVer;
        }

        static int findUser(string path, string user)
        {
            int line = 0;
            int returnLine = -1;
            string verUser = "";
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    var splits = s.Split(',');
                    verUser = splits[0];
                    if (verUser.Equals(user))
                        returnLine = line;
                    line++;
                }
            }
            return returnLine;
        }

        static void sendMail(string userMail, string verCode)
        {
            try
            {
                string s = "";
                string username = "";
                string password = "";
                using (StreamReader sr = File.OpenText("config/loginmail.txt"))
                {
                    s = sr.ReadLine();
                    var splits = s.Split(',');
                    username = splits[0];
                    password = splits[1];
                }
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(username);
                mail.To.Add(userMail);
                mail.Subject = "Verificación como Alumnx UNED de GruposUnedBot";
                mail.Body = String.Concat("Aquí tienes tu código de verificación. Para verificarte, tienes que enviarme un mensaje nuestro chat privado de telegram con el formato \"code:CODIG\"" +
                    "\n\nTu código es:\n" +
                    "code:",verCode);
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
                SmtpServer.EnableSsl = true;
                using (SmtpClient client = new SmtpClient())
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    client.Send(mail);
                    Console.WriteLine("mail Sent");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static string Base64UrlEncode(string input)
        {
            var bytes = System.Text.Encoding.Default.GetBytes(input);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
        static string Base64UrlDecode(string input)
        {
            StringBuilder sbBase64 = new StringBuilder(input);
            sbBase64.Replace("-", "+").Replace("_", "/");
            for (int i = 0; i < (4 - sbBase64.Length % 4) % 4; i++)
                sbBase64.Append("=");
            var bytes = Convert.FromBase64String(sbBase64.ToString());
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

    }
}
