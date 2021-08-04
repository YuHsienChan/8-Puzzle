using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApplication1.Form1;
using System.Threading;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int movesnum = 0, labelIndex = 0;
        List<int> labellist = new List<int>();

        private void shufffle()
        {
            labellist.Clear();

            Random r = new Random();
            foreach (Button btn in this.panel1.Controls)
            {
                while (labellist.Contains(labelIndex))
                    labelIndex = r.Next(9);

                btn.Text = (labelIndex == 0) ? "0" : labelIndex + "";
                btn.BackColor = (btn.Text == "0") ? Color.White : Color.FromKnownColor(KnownColor.ControlLight);
                labellist.Add(labelIndex);
            }
            
            movesnum = 0;
            NbOfMoves.Text = "Number of Moves : " + movesnum;
        }

        private void swap(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.Text == "0")
                return;

            Button whitebtn = null;
            foreach (Button bt in this.panel1.Controls)
            {
                if (bt.Text == "0")
                {
                    whitebtn = bt;
                    break;
                }
            }

            if (btn.TabIndex == (whitebtn.TabIndex - 1) || btn.TabIndex == (whitebtn.TabIndex - 3) || btn.TabIndex == (whitebtn.TabIndex + 1) || btn.TabIndex == (whitebtn.TabIndex + 3))
            {
                whitebtn.BackColor = Color.FromKnownColor(KnownColor.ControlLight);
                btn.BackColor = Color.White;
                whitebtn.Text = btn.Text;
                btn.Text = "0";
                movesnum++;
                NbOfMoves.Text = "Number of Moves : " + movesnum;
            }
            checkOrder();
        }

        private void checkOrder()
        {
            int index = 0;
            foreach (Button btn in this.panel1.Controls)
            {
                if (btn.Text != "0" && Convert.ToInt16(btn.Text) != index)
                {
                    return;
                }
                index++;
            }
            MessageBox.Show("恭喜！完成！你移動第" + movesnum + "次而已耶！");
        }

        private void NewGame_Click(object sender, EventArgs e)
        {
            shufffle();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            shufffle();
        }

        public class Node
        {
            public byte[] status; // 當前狀態
            public Node father; // 紀錄上個狀態，如果father = null 該點為根節點
            public Node(byte[] status, Node father)
            {
                this.status = status;
                this.father = father;
            }

            // 把陣列轉換成數字序列，比對時就不用兩將陣列元素一一檢查了
            // 像是 stauts = {8, 6, 4, 0, 7, 2, 5, 1, 3}，轉後後即可得到864072513
            public int ToSequence()
            {
                int result = 0;
                for (int i = 0; i < status.Length; i++)
                    result = result * 10 + status[i];
                return result;
            }
        }

        List<Node> GetNext(Node now)// 傳入當前版面，回傳0所有移動後的狀態
        {
            int index = Array.IndexOf<byte>(now.status, 0);
            int col = index % 3;
            int row = index / 3;

            List<Node> nextPush = new List<Node>();
            byte[] next;
            byte temp;

            if (row != 0) // Top
            {
                next = (byte[])now.status.Clone();
                //swap(ref next[index], ref next[index - 3]);// 跟上面交換   

                temp = next[index];
                next[index] = next[index - 3];
                next[index - 3] = temp;

                nextPush.Add(new Node(next, now));// 加入這個新狀態
            }

            if (col != 2) // Right
            {
                next = (byte[])now.status.Clone();
                //swap(ref next[index], ref next[index + 1]);// 跟右邊交換

                temp = next[index];
                next[index] = next[index + 1];
                next[index + 1] = temp;

                nextPush.Add(new Node(next, now));
            }

            if (row != 2) // Bottom
            {
                next = (byte[])now.status.Clone();
                //swap(ref next[index], ref next[index + 3]);// 跟下面交換

                temp = next[index];
                next[index] = next[index + 3];
                next[index + 3] = temp;

                nextPush.Add(new Node(next, now));
            }

            if (col != 0) // Left
            {
                next = (byte[])now.status.Clone();
               // swap(ref next[index], ref next[index - 1]);// 跟左邊交換

                temp = next[index];
                next[index] = next[index - 1];
                next[index - 1] = temp;

                nextPush.Add(new Node(next, now));
            }

            return nextPush;
        }

        // 傳入原本的版面和目標版面，回傳最短路徑
        List<Node> Solve(byte[] source, byte[] goal)
        {
            Queue<Node> queue = new Queue<Node>();

            // 使用狀態序列來儲存已走過的路徑，防止往回走
            SortedList<int, bool> book = new SortedList<int, bool>();

            Node end = new Node(goal, null);// 終點
            Node start = new Node(source, null);// 起點

            queue.Enqueue(start);// 推入起點
            book.Add(start.ToSequence(), true);// 標示起點已走過，防止走回頭路

            int endStatus = end.ToSequence();
            while (queue.Count > 0)
            {
                // 取得當前搜索狀態，並移出佇列
                Node now = queue.Dequeue();

                // 如果抵達終點，那就輸出路徑
                if (now.ToSequence() == endStatus)
                    return PathTrace(now);

                // 取得能走的位置
                List<Node> nextPath = GetNext(now);
                foreach (var path in nextPath)
                {
                    int sign = path.ToSequence();

                    // 判斷當前節點狀態是否擴展過了
                    if (!book.Keys.Contains(sign))
                    {
                        // 推入當前狀態，並標記該路徑已走過，因為每個狀態只需要擴展一次就夠了                
                        queue.Enqueue(path);
                        book.Add(sign, true);
                    }
                }
            }

            // 如果窮舉完都沒找到，代表無解
            return null;
        }
        public String str = "";
        public String[,] buttonstr = new string[1000,9];
        public int n = 0;

        List<Node> PathTrace(Node now)
        {
            // 回朔路徑
            List<Node> path = new List<Node>();
            

            while (now.father != null)
            {
                path.Add(now);
                now = now.father;

                str += now.status[0].ToString() + now.status[1].ToString() + now.status[2].ToString() + "\n" + now.status[3].ToString() + now.status[4].ToString() + now.status[5].ToString() + "\n" + now.status[6].ToString() + now.status[7].ToString() + now.status[8].ToString() + "\n";
                str += "---------------------------" + "\n";

                for (int i =0;i<=8;i++)
                {
                    buttonstr[n, i] = now.status[i].ToString();
                }

                n += 1;
            }
            path.Reverse();
            return path;
        }


        private void button10_Click(object sender, EventArgs e)
        {
            button1.Text = labellist[8].ToString();
            button2.Text = labellist[7].ToString();
            button3.Text = labellist[6].ToString();
            button4.Text = labellist[5].ToString();
            button5.Text = labellist[4].ToString();
            button6.Text = labellist[3].ToString();
            button7.Text = labellist[2].ToString();
            button8.Text = labellist[1].ToString();
            button9.Text = labellist[0].ToString();
            //以上為按鈕相對應的值

            byte[] source = new byte[] { byte.Parse(button1.Text), byte.Parse(button2.Text), byte.Parse(button3.Text), byte.Parse(button4.Text), byte.Parse(button5.Text), byte.Parse(button6.Text), byte.Parse(button7.Text), byte.Parse(button8.Text), byte.Parse(button9.Text) };
            byte[] goal = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 0 }; //答案
            
            List<Node> path = Solve(source, goal);

            //MessageBox.Show(buttonstr[0,0]);
           
            for (int i = 2; i <= n; i++)
            {
                MessageBox.Show("第"+(i-1).ToString()+"步");
                button1.Text = buttonstr[n - i, 0];
                button2.Text = buttonstr[n - i, 1];
                button3.Text = buttonstr[n - i, 2];
                button4.Text = buttonstr[n - i, 3];
                button5.Text = buttonstr[n - i, 4];
                button6.Text = buttonstr[n - i, 5];
                button7.Text = buttonstr[n - i, 6];
                button8.Text = buttonstr[n - i, 7];
                button9.Text = buttonstr[n - i, 8];
                Thread.Sleep(1000);
                
            }

            StreamWriter strr = new StreamWriter(@"C:\Users\user\OneDrive\桌面\tree.txt"); //順序是倒過來的，所以步驟是由下而上
            string WriteWord = str;
            strr.WriteLine(WriteWord);
            strr.Close();

            //-----------------------------------------------------------------
            byte UTF8bytes1 = 6;
            string UTF8String1 = UTF8bytes1.ToString(); //位元轉字串OK
            //-----------------------------------------------------------------

            //-----------------------------------------------------------------
            string UTF8String = "6"; 
            byte UTF8bytes =  byte.Parse(UTF8String);  //字串轉位元OK
            //-----------------------------------------------------------------
            MessageBox.Show("最少步驟為:" + (n+1) + "步");
        }
    }  
}
