using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OthellGame
{
    public partial class MainWindow : Window
    {
        private const int BoardSize = 8; // オセロの盤面サイズ（8x8）
        private const int CellSize = 50; // 1マスの大きさ（ピクセル）
        private bool isBlackTurn = true; // true: 黒のターン, false: 白のターン
        private int[,] board = new int[BoardSize, BoardSize]; // 0: 空, 1: 黒, 2: 白

        public MainWindow()
        {
            InitializeComponent();
            DrawBoard(); // 盤面を描画
            InitializeBoard();//初期駒を配置
        }
         private void DrawBoard()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    // マスを作成
                    Rectangle cell = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Stroke = Brushes.Black, // マスの枠線
                        Fill = Brushes.Green    // マスの色
                    };

                    // Canvas 上の座標を設定
                    Canvas.SetLeft(cell, col * CellSize);
                    Canvas.SetTop(cell, row * CellSize);

                    // Canvas に追加
                    GameCanvas.Children.Add(cell);
                }
            }
        }
        private void InitializeBoard()
        {
            // 初期駒配置
            board[3, 3] = board[4, 4] = 2;  // 白
            board[3, 4] = board[4, 3] = 1;  // 黒

            // 初期駒を表示
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] != 0) // 空でないマスには駒が置かれている
                    {
                        // 駒を描画
                        Ellipse piece = new Ellipse
                        {
                            Width = CellSize - 5,
                            Height = CellSize - 5,
                            Fill = board[row, col] == 1 ? Brushes.Black : Brushes.White // 黒または白
                        };

                        // 位置を設定
                        Canvas.SetLeft(piece, col * CellSize + 2.5);
                        Canvas.SetTop(piece, row * CellSize + 2.5);

                        // キャンバスに追加
                        GameCanvas.Children.Add(piece);
                    }
                }
            }
        }
        private void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // クリック位置を取得
            Point clickPosition = e.GetPosition(GameCanvas);
            int col = (int)(clickPosition.X / CellSize);
            int row = (int)(clickPosition.Y / CellSize);

            // 駒を配置
            PlacePiece(row, col);
        }

        private void PlacePiece(int row, int col)
        {
            // すでに駒がある場合は何もしない
            if (board[row, col] != 0) return;

            int player = isBlackTurn ? 1 : 2;

             // ひっくり返せるか判定
            bool canPlace = false;

            List<int[]> directions = new List<int[]>
            {
                new int[] {-1, 0}, new int[] {1, 0}, new int[] {0, -1}, new int[] {0, 1},
                new int[] {-1, -1}, new int[] {-1, 1}, new int[] {1, -1}, new int[] {1, 1}
            };
            foreach (var dir in directions)
            {
                if (CanFlip(row, col, dir[0], dir[1], player))
                {
                    canPlace = true;
                    break;
                }
            }

            if (!canPlace) return; // ひっくり返せないなら置けない


            // 駒を作成
            Ellipse piece = new Ellipse
            {
                Width = CellSize - 5,
                Height = CellSize - 5,
                Fill = isBlackTurn ? Brushes.Black : Brushes.White // 交互に色を変える
            };

            // 位置を設定
            Canvas.SetLeft(piece, col * CellSize + 2.5);
            Canvas.SetTop(piece, row * CellSize + 2.5);

            // キャンバスに追加
            GameCanvas.Children.Add(piece);

            // 盤面情報を更新
            board[row, col] = isBlackTurn ? 1 : 2;

            // ひっくり返す処理
            FlipPieces(row, col, player);

            // 次のターンへ
            NextTurn();
        }

        private bool CanFlip(int row, int col, int dRow, int dCol, int player)
        {
            int r = row + dRow;
            int c = col + dCol;
            bool hasOpponent = false;

            while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize)
            {
                if (board[r, c] == 0) return false; // 空マスなら終了
                if (board[r, c] == player) return hasOpponent; // 挟める場合はtrue

                hasOpponent = true; // 相手の駒がある
                r += dRow;
                c += dCol;
            }
            return false;
        }

        private void FlipPieces(int row, int col, int player)
        {
            List<int[]> directions = new List<int[]>
            {
                new int[] {-1, 0}, new int[] {1, 0}, new int[] {0, -1}, new int[] {0, 1},
                new int[] {-1, -1}, new int[] {-1, 1}, new int[] {1, -1}, new int[] {1, 1}
            };
            foreach (var dir in directions)
            {
                int dRow = dir[0], dCol = dir[1];

                if (CanFlip(row, col, dRow, dCol, player))
                {
                    int r = row + dRow;
                    int c = col + dCol;

                    while (board[r, c] != player)
                    {
                        board[r, c] = player; // ひっくり返す\

                        // UI 上の駒の色も更新
                        foreach (var child in GameCanvas.Children)
                        {
                            if (child is Ellipse piece)
                            {
                                double left = Canvas.GetLeft(piece);
                                double top = Canvas.GetTop(piece);

                                if (Math.Abs(left - (c * CellSize + 2.5)) < 1 && Math.Abs(top - (r * CellSize + 2.5)) < 1)
                                {
                                    piece.Fill = player == 1 ? Brushes.Black : Brushes.White; // 駒の色を変更
                                }
                            }
                        }
                        r += dRow;
                        c += dCol;
                    }
                }
            }
        }
        private bool CanPlayerMove(int player)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] == 0) // 空きマスのみチェック
                    {
                        foreach (var dir in new List<int[]>
                        {
                            new int[] {-1, 0}, new int[] {1, 0}, new int[] {0, -1}, new int[] {0, 1},
                            new int[] {-1, -1}, new int[] {-1, 1}, new int[] {1, -1}, new int[] {1, 1}
                        })
                        {
                            if (CanFlip(row, col, dir[0], dir[1], player))
                            {
                                return true; // ひとつでも合法手があればtrue
                            }
                        }
                    }
                }
            }
            return false; // 置ける場所がない
        }
        private void NextTurn()
        {
            isBlackTurn = !isBlackTurn; // ターンを交代
            int nextPlayer = isBlackTurn ? 1 : 2;

            if (!CanPlayerMove(nextPlayer)) // 次のプレイヤーが置けない場合
            {
                isBlackTurn = !isBlackTurn; // もう一度元のプレイヤーに戻す
                nextPlayer = isBlackTurn ? 1 : 2;

                if (!CanPlayerMove(nextPlayer)) // もう一度チェック（両者とも置けない）
                {
                    EndGame(); // ゲーム終了
                }
            }
        }
        private void EndGame()
        {
            int blackCount = 0, whiteCount = 0;

            // 盤面の駒を数える
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] == 1) blackCount++;
                    if (board[row, col] == 2) whiteCount++;
                }
            }

            string resultMessage = $"黒: {blackCount} - 白: {whiteCount}\n";

            if (blackCount > whiteCount)
            {
                resultMessage += "黒の勝ち！";
            }
            else if (whiteCount > blackCount)
            {
                resultMessage += "白の勝ち！";
            }
            else
            {
                resultMessage += "引き分け！";
            }

            MessageBox.Show(resultMessage, "ゲーム終了");

            // ゲームリセットの処理（必要なら実装）
        }
    }
}