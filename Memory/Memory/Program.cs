using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Aiv.Draw;


namespace Memory
{
    struct Deck
    {
        public Card[] Cards;
        public int TotCards;
        public int OrizCards;
        public int VerCards;
        public int CardSpacing;
        public Vector2 Size;
    }
    struct Card
    {
        public Vector2 Position;
        public Color CurrentColor;
        public Color Color;
        public Color BackColor;
        public Color HoverColor;
        public bool IsTurned;
        public int Id;
    }
    struct WinCheck
    {
        public Card Card1;
        public Card Card2;
        public int ClickIndex;
        public bool ColorCheck;
        public int CorrectGuesses;
    }
    struct Vector2
    {
        public int X;
        public int Y;
    }
    struct Color
    {
        public byte R;
        public byte G;
        public byte B;
    }
    struct Timer
    {
        public float Time;
        public int MaxTime;
        public bool Run;
        public bool Alarm;
    }
    internal class Program
    {
        static void PutPixel(Window w, int x, int y, Color c) 
        {
            if ((x<0 || x>= w.Width) || (y<0 || y>= w.Height))
            {
                return;
            }

            int index = (x + y * w.Width) * 3;

            w.Bitmap[index] = c.R; 
            w.Bitmap[index+1] = c.G; 
            w.Bitmap[index+2] = c.B; 
        }

        static void PutPixel(Window w, int x, int y, int r, int g, int b)
        {
            if ((x < 0 || x >= w.Width) || (y < 0 || y >= w.Height))
            {
                return;
            }

            int index = (x + y * w.Width) * 3;

            w.Bitmap[index] = (byte)r;
            w.Bitmap[index + 1] = (byte)g;
            w.Bitmap[index + 2] = (byte)b;
        }
        static void ClearScreen(Window w)
        {
            for (int i = 0; i < w.Bitmap.Length; i++)
            {
                w.Bitmap[i] = 0;
            }
        }
        static void InitGrid(out Deck deck, int w, int h, int spa)
        {
            deck.TotCards = w * h;
            deck.OrizCards = w;
            deck.VerCards = h;
            deck.CardSpacing = spa;
            deck.Size.X = 100;
            deck.Size.Y = 100;
            deck.Cards = new Card[deck.TotCards];
            int startPos = 150;
            int x;
            int y;
            int id;
            Color color;
            for (int i = 0; i < deck.TotCards; i++)
            {
                x = startPos + ((i % deck.OrizCards) * (deck.Size.X + spa));
                y = startPos + ((i / deck.OrizCards) * (deck.Size.Y + spa));

                id = (int)(i * 0.5f);
                color = GetRandomColor(id);

                InitCard(out deck.Cards[i], x, y, id, color);
            }
        }
        static void InitCard(out Card card, int x, int y, int id, Color col)
        {
            card.Position.X = x;
            card.Position.Y = y;
            card.Id = id;
            card.IsTurned = false;
            card.Color = col;
            card.BackColor.R = 0;
            card.BackColor.G = 0; 
            card.BackColor.B = 0;
            card.HoverColor.R = 100;
            card.HoverColor.G = 100;
            card.HoverColor.B = 100;
            card.CurrentColor = card.BackColor;
        }
        static void InitWincond(out WinCheck win)
        {
            InitCard(out win.Card1, 0, 0, 0, GetRandomColor(0));
            InitCard(out win.Card2, 0, 0, 0, GetRandomColor(0));
            win.ColorCheck = false;
            win.ClickIndex = 0;
            win.CorrectGuesses = 0;
        }
        static Color GetRandomColor(int seed)
        {
            Random random = new Random(seed);
            Color c;
            c.R = (byte)random.Next(0, 256);
            c.G = (byte)random.Next(0, 256);
            c.B = (byte)random.Next(0, 256);
            return c;
        }

        static void DeckShuffle(ref Deck deck)
        {
            Random random = new Random();

            for (int i = deck.TotCards - 1; i > 0; i--)
            {
                int index = random.Next(0, i + 1);

                Card temp = deck.Cards[i];
                deck.Cards[i] = deck.Cards[index];
                deck.Cards[index] = temp;

                Vector2 temp1 = deck.Cards[i].Position;
                deck.Cards[i].Position = deck.Cards[index].Position;
                deck.Cards[index].Position = temp1;
            }
        }

        static void InitTimer(out Timer timer)
        {
            timer.Time = 0.0f;
            timer.MaxTime = 0;
            timer.Alarm = false;
            timer.Run = false;
        }

        static void Main(string[] args)
        {
            Window window = new Window(1000, 800, "Memory", PixelFormat.RGB);

            Deck deck;
            WinCheck win;
            Timer timer;
            InitGame(out deck, out win, out timer);

            while (window.IsOpened)
            {
                
                if (win.CorrectGuesses == deck.TotCards * 0.5f)
                {
                    Thread.Sleep(1000);
                    InitGame(out deck, out win, out timer);
                }
                if (timer.Run)
                {
                    timer.Alarm = Timer(ref timer, 1, window);
                    if (timer.Alarm)
                    {
                        timer.Alarm = false;
                        timer.Time = 0.0f;
                        timer.Run = false;
                        for (int i = 0; i < deck.TotCards; i++)
                        {
                            if (deck.Cards[i].Id == win.Card1.Id || deck.Cards[i].Id == win.Card2.Id)
                            {
                                deck.Cards[i].CurrentColor = deck.Cards[i].BackColor;
                                deck.Cards[i].IsTurned = false;
                            }
                        }
                    }
                }
                else
                {
                    CheckHover(window, ref deck);
                    if (win.ClickIndex == 0)
                    {
                        CheckInput(window, deck, ref win.Card1, ref win);
                    }
                    else if (win.ClickIndex == 1)
                    {
                        if (CheckInput1(window, deck, ref win.Card2))
                        {
                            if (win.Card1.Id == win.Card2.Id)
                            {
                                win.CorrectGuesses++;
                                win.ClickIndex = 0;
                            }
                            else
                            {
                                timer.Run = true;
                                win.ClickIndex = 0;
                            }
                        }
                    }
                }

                ClearScreen(window);

                DrawDeck(window, deck);
                

                window.Blit();
            }
        }

        static void InitGame(out Deck deck, out WinCheck winCheck, out Timer timer)
        {
            InitGrid(out deck, 5, 4, 50);
            DeckShuffle(ref deck);

            InitWincond(out winCheck);

            InitTimer(out timer);
        }

        static bool Timer(ref Timer timer, int t, Window w)
        {
            timer.MaxTime = t;
            timer.Time += w.DeltaTime;
            return timer.Time >= timer.MaxTime;
        }
        static void CheckInput(Window w, Deck deck ,ref Card card1, ref WinCheck win)
        {
            if (w.MouseLeft)
            {
                for (int i = 0; i < deck.TotCards; i++)
                {
                    int x = w.MouseX;
                    int y = w.MouseY;
                    if (x > deck.Cards[i].Position.X && x < deck.Cards[i].Position.X + deck.Size.X && y > deck.Cards[i].Position.Y && y < deck.Cards[i].Position.Y + deck.Size.Y && deck.Cards[i].IsTurned == false)
                    {
                        deck.Cards[i].CurrentColor = deck.Cards[i].Color;
                        deck.Cards[i].IsTurned = true;
                        card1 = deck.Cards[i];
                        win.ClickIndex++;
                    }
                }
                
            }
        }
        static bool CheckInput1(Window w, Deck deck, ref Card card1)
        {
            if (w.MouseLeft)
            {
                for (int i = 0; i < deck.TotCards; i++)
                {
                    int x = w.MouseX;
                    int y = w.MouseY;
                    if (x > deck.Cards[i].Position.X && x < deck.Cards[i].Position.X + deck.Size.X && y > deck.Cards[i].Position.Y && y < deck.Cards[i].Position.Y + deck.Size.Y && deck.Cards[i].IsTurned == false)
                    {
                        deck.Cards[i].CurrentColor = deck.Cards[i].Color;
                        deck.Cards[i].IsTurned = true;
                        card1 = deck.Cards[i];
                        return true;
                    }
                }

            }
            return false;
        }
        static bool CheckHover(Window w, ref Deck deck)
        {
            int x = w.MouseX;
            int y = w.MouseY;

            for (int i = 0; i < deck.TotCards; i++)
            {
                if (x > deck.Cards[i].Position.X && x < deck.Cards[i].Position.X + deck.Size.X && y > deck.Cards[i].Position.Y && y < deck.Cards[i].Position.Y + deck.Size.Y && deck.Cards[i].IsTurned == false)
                {
                    deck.Cards[i].CurrentColor = deck.Cards[i].HoverColor;
                    return true;
                }
                else if (deck.Cards[i].IsTurned)
                {
                    deck.Cards[i].CurrentColor = deck.Cards[i].Color;
                }
                else
                {
                    deck.Cards[i].CurrentColor = deck.Cards[i].BackColor;
                }
            }
            return false;
        }

        static void DrawDeck(Window w, Deck deck)
        {
            for (int i = 0; i < deck.TotCards; i++)
            {
                DrawCard(w, deck.Cards[i], deck);
            }
        }
        static void DrawCard(Window w, Card card, Deck deck)
        {
            for (int i = 0; i < deck.Size.Y; i++)
            {
                for (int t = 0; t < deck.Size.X; t++)
                {
                    if ( i == 0 || i == deck.Size.Y - 1 || t == 0 || t == deck.Size.X - 1)
                    {
                        PutPixel(w, card.Position.X + t, card.Position.Y + i, 255, 255, 255);
                    }
                    else
                    {
                        PutPixel(w, card.Position.X + t, card.Position.Y + i, card.CurrentColor);
                    }
                }
            }
        }
    }
}
