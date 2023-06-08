using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicToc
{
    public partial class Main : Form
    {
        //Block input flag (Avoid unwanted behaviours)
        internal bool BlockInput = true;

        //CPU Player BlockInput flag
        private bool cpuPlaying = true;
        private bool cpu = false;

        //CPU Level
        private short level = 3;

        //Circle/Cross flag
        private bool circle = false;

        //Score
        private int p1 = 0;
        private int p2 = 0;

        //Menu song player
        private System.Windows.Media.MediaPlayer myPlayer = new System.Windows.Media.MediaPlayer();

        //Winning patterns
        private readonly short[] winning =
        {
            //Horizontal (3x3)
            0,1,2,
            3,4,5,
            6,7,8,

            //Vertical (3x3)
            0,3,6,
            1,4,7,
            2,5,8,

            //Diagonal (3x2)
            0,4,8,
            2,4,6
        };

        //Square values mirror (3x3)
        //0 1 2
        //3 4 5
        //7 8 9
        private short[] values = new short[9];

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //Set default values
            DefaultValues();

            //Define Title
            Text = "Tic-Tac-Toc AI By Starlyn1232";
        }

        //Shown event
        private void Main_Shown(object sender, EventArgs e)
        {
            //Init Music
            PlayMusic();
        }

        //Play Sound
        private void PlaySound(Stream sound)
        {
            SoundPlayer pl = new SoundPlayer();

            pl.Stream = sound;

            pl.Play();
        }

        //Process Pic Click
        private void PicClick(PictureBox pic, bool state = false)
        {
            Task.Run(() =>
            {
                //Skip existing set
                if (pic.Image != null || BlockInput)
                    return;

                //Temp value
                short val = (short)(circle ? 1 : 0);

                //Set flag
                if (pic.Name == "picBox1")
                    values[0] = val;

                else if (pic.Name == "picBox2")
                    values[1] = val;

                else if (pic.Name == "picBox3")
                    values[2] = val;

                else if (pic.Name == "picBox4")
                    values[3] = val;

                else if (pic.Name == "picBox5")
                    values[4] = val;

                else if (pic.Name == "picBox6")
                    values[5] = val;

                else if (pic.Name == "picBox7")
                    values[6] = val;

                else if (pic.Name == "picBox8")
                    values[7] = val;

                else if (pic.Name == "picBox9")
                    values[8] = val;

                //Define image
                pic.Image = circle ?
                    Properties.Resources.circle :
                    Properties.Resources.cross;

                //Check winner (Won sound)
                if (CheckWinner())
                    DefaultValues();

                //Check Game Over (Click Sound)
                else
                {
                    if (!CheckCompleted())
                        PlaySound(Properties.Resources.click);
                }

                //Invert value
                circle = !circle;

                //Run NCP
                cpu = state;

                if (cpu && cpuPlaying)
                    ExtraHandler(PlayerAI() + 1);
            });
        }

        //Invoked callback
        private void InvokeState(Panel ctrl, bool state)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke(new Action(() => ctrl.Enabled = state));

            else
                ctrl.Enabled = state;
        }

        private void InvokeState(PictureBox ctrl, bool state)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke(new Action(() => ctrl.Visible = state));

            else
                ctrl.Visible = state;
        }

        //Clear Fields
        private void DefaultValues()
        {
            //Clear pictures
            picBox1.Image = null;
            picBox2.Image = null;
            picBox3.Image = null;
            picBox4.Image = null;
            picBox5.Image = null;
            picBox6.Image = null;
            picBox7.Image = null;
            picBox8.Image = null;
            picBox9.Image = null;

            //Reset values
            for (int i = 0; i < 9; i++)
                values[i] = -1;

            //Free RAM
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        //All fields
        private bool CheckCompleted()
        {
            if (picBox1.Image != null && picBox2.Image != null && picBox3.Image != null &&
                picBox4.Image != null && picBox5.Image != null && picBox6.Image != null &&
                picBox7.Image != null && picBox8.Image != null && picBox9.Image != null)
            {
                //Random no-winner sounds
                var ran = new Random().Next(0, 4);

                switch (ran)
                {
                    case 0:
                        PlaySound(Properties.Resources.pedo);
                        break;
                    case 1:
                        PlaySound(Properties.Resources.no_winner_2);
                        break;
                    case 2:
                        PlaySound(Properties.Resources.no_winner_3);
                        break;
                    case 3:
                        PlaySound(Properties.Resources.no_winner_4);
                        break;
                }

                Thread.Sleep(1200);

                DefaultValues();

                return true;
            }

            return false;
        }

        //Check Winner
        private bool CheckWinner()
        {
            bool winner = false;

            //Check winning patterns
            for (int i = 2; i <= 24; i += 3)
            {
                if (CheckPatterns(values[winning[i]], values[winning[i - 1]], values[winning[i - 2]]))
                {
                    winner = true;

                    break;
                }
            }

            //Check if someone won
            if (winner)
            {
                //Remove player 2 heart
                if (circle)
                    p1--;

                //Remove player 1 heart
                else
                    p2--;

                //Update hearts
                if (p1 < 3)
                    Hearts(false, p1 + 1);

                if (p2 < 3)
                    Hearts(false, p2 + 4);

                if (p1 == 0 || p2 == 0)
                {
                    //Avoid inputs
                    BlockInput = true;

                    PlaySound(Properties.Resources.won);
                    InvokeState(picWinning, true);

                    //Disable Cancel button
                    btnCancel.Invoke(() =>
                    {
                        btnCancel.Enabled = false;
                    });

                    MessageBox.Show($"The winner is the {(circle ? "circle" : "cross")} player!!!", "Tic-Tac-Toc AI by Starlyn1232",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    InvokeState(picWinning, false);

                    EndGame();
                }

                else
                {
                    //Avoid inputs
                    BlockInput = true;
                    InvokeState(picWinning, true);

                    PlaySound(Properties.Resources.round_win);
                    Thread.Sleep(1500);

                    //Allow inputs
                    BlockInput = false;

                    InvokeState(picWinning, false);
                }
            }

            return winner;
        }

        //Check patterns
        private bool CheckPatterns(params int[] patterns)
        {
            int len = 3, i = 0, o = 0, x = 0;

            for (i = 0; i < len; i++)
            {
                //Check Circles
                if (patterns[i] == 1)
                    o++;

                //Check Crosses
                else if (patterns[i] == 0)
                    x++;
            }

            return o == len || x == len;
        }

        //Squares click events
        private void picBox1_Click(object sender, EventArgs e)
        {
            PicClick(picBox1, cpuPlaying);
        }

        private void picBox2_Click(object sender, EventArgs e)
        {
            PicClick(picBox2, cpuPlaying);
        }

        private void picBox3_Click(object sender, EventArgs e)
        {
            PicClick(picBox3, cpuPlaying);
        }

        private void picBox4_Click(object sender, EventArgs e)
        {
            PicClick(picBox4, cpuPlaying);
        }

        private void picBox5_Click(object sender, EventArgs e)
        {
            PicClick(picBox5, cpuPlaying);
        }

        private void picBox6_Click(object sender, EventArgs e)
        {
            PicClick(picBox6, cpuPlaying);
        }

        private void picBox7_Click(object sender, EventArgs e)
        {
            PicClick(picBox7, cpuPlaying);
        }

        private void picBox8_Click(object sender, EventArgs e)
        {
            PicClick(picBox8, cpuPlaying);
        }

        private void picBox9_Click(object sender, EventArgs e)
        {
            PicClick(picBox9, cpuPlaying);
        }

        //Key event
        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (!BlockInput)
            {
                //Play with numbers keys
                if (Char.IsDigit((char)e.KeyValue))
                    ExtraHandler(Convert.ToInt32(e.KeyValue - '0'), cpuPlaying);

                //End the game using F2 key
                else if (e.KeyCode == Keys.F2)
                    EndGame();
            }

            else
            {
                //Start the game using F1 key
                if (e.KeyCode == Keys.F1 && btnPlay.Enabled)
                    StartGame();
            }
        }

        //Extra Event Handler
        private void ExtraHandler(int index, bool state = false)
        {
            cpu = state;

            if (index == 1)
                PicClick(picBox1, state);

            else if (index == 2)
                PicClick(picBox2, state);

            else if (index == 3)
                PicClick(picBox3, state);

            else if (index == 4)
                PicClick(picBox4, state);

            else if (index == 5)
                PicClick(picBox5, state);

            else if (index == 6)
                PicClick(picBox6, state);

            else if (index == 7)
                PicClick(picBox7, state);

            else if (index == 8)
                PicClick(picBox8, state);

            else if (index == 9)
                PicClick(picBox9, state);
        }

        //Let's prepare a IA that can defeat you...
        private short PlayerAI()
        {
            //Disable inputs
            BlockInput = true;
            InvokeState(panelGame, false);

            //Random value generator
            Random random = new();

            //Wait for a random milliseconds ET
            //0.7 to 2.6 seconds
            var ran = random.Next(7, 26);

            if (level < 3)
                Thread.Sleep(400);

            else
                Thread.Sleep(100 * ran);

            //Short list for non-winning square selection
            List<short> squares = new List<short>();

            //Needed variables for calculations
            int len = 3, i = 0, o = 0, x = 0;
            short[] val = new short[3];
            short circle = -1;
            short cross = -1;

            //Check last value
            for (short e = 2; e <= 24; e += 3)
            {
                val = new short[]
                {
                    values[winning[e]],
                    values[winning[e - 1]],
                    values[winning[e - 2]]
                };

                for (i = 0, o = 0, x = 0; i < len; i++)
                {
                    //Get circle winning move
                    if (val[i] == 1)
                        o++;

                    //Get cross winning move
                    else if (val[i] == 0)
                        x++;

                    //Save empty squares
                    else
                        squares.Add(winning[e - i]);

                    if (o == 2)
                    {
                        for (i = 0; i < len; i++)
                        {
                            //The unique empty square
                            if (val[i] == -1)
                            {
                                //Notify shamefull truth
                                if (circle != -1)
                                {
                                    //More than one chance to win!
                                    if (this.circle && level > 1)
                                        MessageBox.Show("OMG! Winning is unstoppable!", "Tic-Tac-Toc AI by Starlyn1232",
                                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }

                                //Save the value
                                else
                                    circle = winning[e - i];
                            }
                        }

                        break;
                    }

                    else if (x == 2)
                    {
                        for (i = 0; i < len; i++)
                        {
                            //The unique empty square
                            if (val[i] == -1)
                            {
                                //Notify shamefull truth
                                if (cross != -1)
                                {
                                    //More than one chance to win!
                                    if (!this.circle && level > 1)
                                        MessageBox.Show("OMG! Winning is unstoppable!", "Tic-Tac-Toc AI by Starlyn1232",
                                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }

                                //Save the value
                                else
                                    cross = winning[e - i];
                            }
                        }

                        break;
                    }
                }

                //Check if we got both values
                if (cross != -1 && circle != -1)
                    break;
            }

            short result = -1;

            //Win / Avoid opponent win
            if (level > 1)
            {
                if (this.circle)
                {
                    if (circle != -1)
                        result = circle;

                    else if (cross != -1)
                        result = cross;
                }

                else
                {
                    if (cross != -1)
                        result = cross;

                    else if (circle != -1)
                        result = circle;
                }

                //Check current result
                if (result > -1)
                    result = winning[result];
            }

            //If nobody will win (yet), then just select some
            //Intelligent square
            if (result == -1 && level > 1)
            {
                //Always take the middle square :)
                if (values[4] == -1)
                    result = 4;

                else
                {
                    //Static assign (avoid trick moves)

                    //Always check the corners, there is
                    //the tricky strategies
                    if
                        (//Left upper
                        values[0] == -1 ||
                        //Right upper
                        values[2] == -1 ||
                        //Left lower
                        values[6] == -1 ||
                        //Right lower
                        values[8] == -1)
                    {
                        squares.Clear();

                        if (values[0] == -1)
                            squares.Add(0);

                        if (values[2] == -1)
                            squares.Add(2);

                        if (values[6] == -1)
                            squares.Add(6);

                        if (values[8] == -1)
                            squares.Add(8);

                        ran = squares.Count;

                        //More than one corner available?
                        //Initialize tricky moves :)
                        if (ran > 1)
                        {
                            //Select specifically the opposive square
                            //only in the specific moment
                            if (ran == 3)
                            {
                                if (values[0] != -1 && values[8] == -1)
                                    result = 8;

                                else if (values[2] != -1 && values[6] == -1)
                                    result = 6;

                                else if (values[6] != -1 && values[2] == -1)
                                    result = 2;

                                else if (values[8] != -1 && values[0] == -1)
                                    result = 0;
                            }

                            else if (ran == 2)
                            {
                                //Choose the correct corner to force the win
                                //sequence
                                if ((this.circle && values[4] == 1) ||
                                    (!this.circle && values[4] == 0))
                                {
                                    //Professional move
                                    if (level == 2 ||
                                        (values[0] != values[8] ||
                                        values[3] != values[7]))
                                    {
                                        //Left upper
                                        if (values[0] == -1 && values[1] == -1 && values[3] == -1)
                                            result = 0;

                                        //Right upper
                                        else if (values[2] == -1 && values[1] == -1 && values[5] == -1)
                                            result = 2;

                                        //right lower
                                        else if (values[8] == -1 && values[5] == -1 && values[7] == -1)
                                            result = 8;

                                        //Left lower
                                        else if (values[6] == -1 && values[3] == -1 && values[7] == -1)
                                            result = 6;
                                    }

                                    //God move :)
                                    else if (level == 3)
                                    {
                                        squares.Clear();

                                        if (values[1] == -1)
                                            squares.Add(1);

                                        if (values[3] == -1)
                                            squares.Add(3);

                                        if (values[5] == -1)
                                            squares.Add(5);

                                        if (values[7] == -1)
                                            squares.Add(7);

                                        ran = random.Next(0, squares.Count);

                                        result = squares[ran];
                                    }
                                }
                            }

                            if (result == -1)
                            {
                                //Select a random corner to make it more
                                //humanized?! ;)
                                ran = (short)random.Next(0, ran);

                                //Define result
                                result = squares[ran];
                            }
                        }

                        else
                            result = squares[0];
                    }
                }
            }

            //Select a random square, level 1?
            if (result == -1)
            {
                //Count empty squares
                int qnt = squares.Count;

                //Get random square
                ran = random.Next(0, qnt);

                //Select random square
                while (values[squares[ran]] != -1)
                    ran = random.Next(0, qnt);

                //Select random square
                result = winning[squares[ran]];
            }

            //Enable back inputs
            BlockInput = false;
            InvokeState(panelGame, true);

            return result;
        }

        //Start the game
        private void StartGame()
        {
            PlayMusic(false);

            //Set values
            DefaultValues();

            //Start sound
            PlaySound(Properties.Resources.start);

            //Show all heart icons
            Hearts(true);

            //Disable level/mode controls
            ControlState(false);

            //Allow input
            BlockInput = false;
            cpuPlaying = picCPU.BackColor == Color.Red;

            //Always start the cross
            this.circle = false;

            //3 lives for each
            p1 = 3;
            p2 = 3;
        }

        //Play button event
        private void btnPlay_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        //Finish the game
        private void EndGame()
        {
            //Set values
            DefaultValues();

            //Show all heart icons
            Hearts(true);

            //Disable level/mode controls
            ControlState(true);

            //Avoid continue BlockInput
            BlockInput = true;
            cpuPlaying = false;

            PlayMusic();
        }

        //Heart Manager
        private void Hearts(bool state, int index = -1)
        {
            if (index == -1)
            {
                InvokeState(picHeart1, state);
                InvokeState(picHeart2, state);
                InvokeState(picHeart3, state);
                InvokeState(picHeart4, state);
                InvokeState(picHeart5, state);
                InvokeState(picHeart6, state);
            }

            else
            {
                switch (index)
                {
                    case 1:
                        InvokeState(picHeart1, state);
                        break;
                    case 2:
                        InvokeState(picHeart2, state);
                        break;
                    case 3:
                        InvokeState(picHeart3, state);
                        break;
                    case 4:
                        InvokeState(picHeart4, state);
                        break;
                    case 5:
                        InvokeState(picHeart5, state);
                        break;
                    case 6:
                        InvokeState(picHeart6, state);
                        break;
                }
            }
        }

        //Mode Selection
        private void ModeSelector(short Mode)
        {
            if (Mode == 1)
            {
                picCPU.BackColor = Color.Red;
                picMAN.BackColor = Color.CornflowerBlue;
            }

            else
            {
                picCPU.BackColor = Color.CornflowerBlue;
                picMAN.BackColor = Color.Red;
            }

            PlaySound(Properties.Resources.selection);
        }

        //Level Selection
        private void LevelSelector(short Level)
        {
            //Disable all
            radioNoob.Checked = false;
            radioPro.Checked = false;
            radioGod.Checked = false;

            switch (Level)
            {
                case 1:
                    radioNoob.Checked = true;
                    break;
                case 2:
                    radioPro.Checked = true;
                    break;
                case 3:
                    radioGod.Checked = true;
                    break;
            }

            this.level = Level;
        }

        //Disable/Enable state radioButtons
        private void ControlState(bool state)
        {
            //Disable all
            Invoke(() =>
            {
                radioNoob.Enabled = state;
                radioPro.Enabled = state;
                radioGod.Enabled = state;

                picCPU.Enabled = state;
                picMAN.Enabled = state;

                btnPlay.Enabled = state;
                btnCancel.Enabled = !state;
            });
        }

        //CPU level selection events
        private void radioNoob_CheckedChanged(object sender, EventArgs e)
        {
            LevelSelector(1);
        }

        private void radioPro_CheckedChanged(object sender, EventArgs e)
        {
            LevelSelector(2);
        }

        private void radioGod_Click(object sender, EventArgs e)
        {
            LevelSelector(3);
        }

        //Play mode events
        private void picCPU_Click(object sender, EventArgs e)
        {
            ModeSelector(1);
        }

        private void picMAN_Click(object sender, EventArgs e)
        {
            ModeSelector(2);
        }

        //Play/Stop Menu song
        private void PlayMusic(bool play = true)
        {
            //For the execution from the main's thread
            Invoke(() =>
            {
                try
                {
                    if (!play)
                    {
                        try
                        {
                            myPlayer.Pause();
                            myPlayer.Stop();
                        }
                        catch { }

                        return;
                    }

                    //Using MediaPlayer
                    try
                    {
                        myPlayer.Pause();
                        myPlayer.Stop();
                    }
                    catch { }

                    myPlayer.Open(new Uri($"{Directory.GetCurrentDirectory()}\\Media\\song.wav"));
                    myPlayer.Play();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                    if (play)
                        MessageBox.Show("Cannot load song!", "Tic-Tac-Toc AI by Starlyn1232",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (BlockInput)
                return;

            EndGame();
        }
    }
}