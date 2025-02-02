using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Comgame.GameObject;

class MovingBubble : Bubble
{
    public Vector2 Velocity;
    public bool HasStopped { get; private set; }
    private int comboDestroyCount = 0;
    private Vector2 lastDestroyedPosition = Vector2.Zero;

    public MovingBubble(Vector2 position) : base(position)
    {
        HasStopped = false;
        CurrentColor = GetRandomColor();
        _texture = _bubbleTextures[CurrentColor];
    }

    public override void Update(GameTime gameTime)
    {
        if (HasStopped)
            return;

        MoveBubble(gameTime);
        CheckCollisionWithWalls();
        CheckCollisionWithBubbles();

        base.Update(gameTime);
    }

    private void MoveBubble(GameTime gameTime)
    {
        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    private void CheckCollisionWithWalls()
    {
        if (Position.X < 0 || Position.X > (Singleton.GAMEWIDTH * Singleton.TILESIZE) - Singleton.TILESIZE)
        {
            Velocity = new Vector2(-Velocity.X, Velocity.Y);
        }
        else if (Position.Y <= 0) // ชนขอบบน
        {
            StopBubble();
        }
    }

    private void CheckCollisionWithBubbles()
    {
        for (int y = 0; y < Singleton.GAMEHEIGHT; y++)
        {
            for (int x = 0; x < Singleton.GAMEWIDTH; x++)
            {
                var otherBubble = Singleton.Instance.GameBoard[x, y];
                if (otherBubble != null && Vector2.Distance(Position, otherBubble.Position) < Singleton.TILESIZE * 0.9f)
                {
                    StopBubble();
                }

                if (HasStopped)
                {
                    Singleton.PrintGameBoard();
                    return;
                }
            }
        }
    }

    private void ActivateBlackhole(int row, int col)
    {
        // destroy surrounding bubbles
        Singleton.Instance.GameBoard[col, row] = null; // the collided blackhole
        if (col - 1 >= 0) Singleton.Instance.GameBoard[col - 1, row] = null; // left
        if (col + 1 < Singleton.GAMEWIDTH) Singleton.Instance.GameBoard[col + 1, row] = null; // right
        if (row - 1 >= 0) Singleton.Instance.GameBoard[col, row - 1] = null; // top
        if (row + 1 < Singleton.GAMEHEIGHT) Singleton.Instance.GameBoard[col, row + 1] = null; // bottom
        if (Singleton.IsRowEven(row))
        {
            if (col - 1 >= 0 && row - 1 >= 0) Singleton.Instance.GameBoard[col - 1, row - 1] = null; // top-left
            if (col - 1 >= 0 && row + 1 < Singleton.GAMEHEIGHT) Singleton.Instance.GameBoard[col - 1, row + 1] = null; // bottom-left
        }
        else
        {
            if (col + 1 < Singleton.GAMEWIDTH && row - 1 >= 0) Singleton.Instance.GameBoard[col + 1, row - 1] = null; // top-right
            if (col + 1 < Singleton.GAMEWIDTH && row + 1 < Singleton.GAMEHEIGHT) Singleton.Instance.GameBoard[col + 1, row + 1] = null; // bottom-right
        }
        DestroyFloatingBubbles(); // ลบ Bubble ที่ลอยอยู่

        Singleton.Instance.BHSound.Play(0.3f, 0.0f, 0.0f);
        Singleton.Instance.Score += 100;
    }

    private void StopBubble()
    {
        HasStopped = true;
        var (col, row) = ConvertPositionToGrid();

        if (IsGameOver(col, row))
            return;

        if (CheckBlackholeCollision(row, col))
            return;

        AdjustBubblePosition(col, row);
        AddBubbleToBoard(col, row);

        if (CheckAndDestroyBubbles(col, row, CurrentColor) >= 3)
        {
            FloodFillDestroy(col, row, CurrentColor);
            DestroyFloatingBubbles(); // ลบ Bubble ที่ลอยอยู่

            Singleton.Instance.Score += comboDestroyCount * 10;
            if (comboDestroyCount > 1)
            {
                // Store combo details in Singleton
                Singleton.Instance.UpdateCombo(comboDestroyCount, lastDestroyedPosition);
            }
        }
        comboDestroyCount = 0;
        Singleton.RenderGameBoard();
    }

    private (int col, int row) ConvertPositionToGrid()
    {
        int col = (int)Math.Round(Position.X / Singleton.TILESIZE);
        int row = (int)Math.Round(Position.Y / (Singleton.TILESIZE * 0.866f)); // 0.866 = sqrt(3)/2

        if (!Singleton.IsRowEven(row) && col == Singleton.GAMEWIDTH - 1)
            col--;

        // Check upper and right-upper cell for odd rows
        if (!Singleton.IsRowEven(row) && row != 0 && Singleton.Instance.GameBoard[col, row - 1] == null && Singleton.Instance.GameBoard[col + 1, row - 1] == null)
        {
            col--;
        }

        return (col, row);
    }

    private bool IsGameOver(int col, int row)
    {
        if (col >= Singleton.GAMEWIDTH || row >= Singleton.GAMEHEIGHT)
        {
            Singleton.Instance.CurrentGameState = Singleton.GameState.GameLose;
            return true;
        }
        return false;
    }

    private bool CheckBlackholeCollision(int row, int col)
    {
        if ((col - 1 >= 0) && Singleton.Instance.GameBoard[col - 1, row] != null && Singleton.Instance.GameBoard[col - 1, row].CurrentColor == BubbleColor.BLACKHOLE)
        {
            ActivateBlackhole(row, col - 1);
            return true;
        }
        if ((col + 1 < Singleton.GAMEWIDTH) && Singleton.Instance.GameBoard[col + 1, row] != null && Singleton.Instance.GameBoard[col + 1, row].CurrentColor == BubbleColor.BLACKHOLE)
        {
            ActivateBlackhole(row, col + 1);
            return true;
        }
        if ((row - 1 >= 0) && Singleton.Instance.GameBoard[col, row - 1] != null && Singleton.Instance.GameBoard[col, row - 1].CurrentColor == BubbleColor.BLACKHOLE)
        {
            ActivateBlackhole(row - 1, col);
            return true;
        }
        if ((row + 1 < Singleton.GAMEHEIGHT) && Singleton.Instance.GameBoard[col, row + 1] != null && Singleton.Instance.GameBoard[col, row + 1].CurrentColor == BubbleColor.BLACKHOLE)
        {
            ActivateBlackhole(row + 1, col);
            return true;
        }
        if (Singleton.IsRowEven(row))
        {
            if ((col - 1 >= 0 && row - 1 >= 0) && Singleton.Instance.GameBoard[col - 1, row - 1] != null && Singleton.Instance.GameBoard[col - 1, row - 1].CurrentColor == BubbleColor.BLACKHOLE)
            {
                ActivateBlackhole(row - 1, col - 1);
                return true;
            }
            if ((col - 1 >= 0 && row + 1 < Singleton.GAMEHEIGHT) && Singleton.Instance.GameBoard[col - 1, row + 1] != null && Singleton.Instance.GameBoard[col - 1, row + 1].CurrentColor == BubbleColor.BLACKHOLE)
            {
                ActivateBlackhole(row + 1, col - 1);
                return true;
            }
        }
        else
        {
            if ((col + 1 < Singleton.GAMEWIDTH && row - 1 >= 0) && Singleton.Instance.GameBoard[col + 1, row - 1] != null && Singleton.Instance.GameBoard[col + 1, row - 1].CurrentColor == BubbleColor.BLACKHOLE)
            {
                ActivateBlackhole(row - 1, col + 1);
                return true;
            }
            if ((col + 1 < Singleton.GAMEWIDTH && row + 1 < Singleton.GAMEHEIGHT) && Singleton.Instance.GameBoard[col + 1, row + 1] != null && Singleton.Instance.GameBoard[col + 1, row + 1].CurrentColor == BubbleColor.BLACKHOLE)
            {
                ActivateBlackhole(row + 1, col + 1);
                return true;
            }
        }
        return false;
    }

    private void AdjustBubblePosition(int col, int row)
    {
        float offsetX = (Singleton.IsRowEven(row)) ? 0 : Singleton.TILESIZE / 2;
        Position = new Vector2(col * Singleton.TILESIZE + offsetX, row * Singleton.TILESIZE * 0.866f);
    }

    private void AddBubbleToBoard(int col, int row)
    {
        if (Singleton.Instance.GameBoard[col, row] == null)
        {
            Singleton.Instance.GameBoard[col, row] = new Bubble(Position, CurrentColor);
        }
        else
        {
            int newRow = row + 1;
            if (newRow < Singleton.GAMEHEIGHT)
            {
                Singleton.Instance.GameBoard[col, newRow] = new Bubble(new Vector2(Position.X, newRow * Singleton.TILESIZE * 0.866f), CurrentColor);
            }
        }
    }

    private void DestroyFloatingBubbles()
    {
        HashSet<(int, int)> connectedToTop = new HashSet<(int, int)>();
        Singleton.Instance.Exploded.Play(0.1f, 0.0f, 0.0f);
        // ตรวจหาว่า Bubble ไหนเชื่อมต่อกับแถวบนสุด
        for (int x = 0; x < Singleton.GAMEWIDTH; x++)
        {
            if (Singleton.Instance.GameBoard[x, 0] != null)
            {
                MarkConnectedBubbles(x, 0, connectedToTop);
            }
        }

        // ลบ Bubble ที่ไม่ได้เชื่อมต่อกับแถวบนสุด
        for (int y = 0; y < Singleton.GAMEHEIGHT; y++)
        {
            for (int x = 0; x < Singleton.GAMEWIDTH; x++)
            {
                if (Singleton.Instance.GameBoard[x, y] != null && !connectedToTop.Contains((x, y)))
                {
                    Singleton.Instance.GameBoard[x, y] = null;
                }
            }
        }
    }

    private void MarkConnectedBubbles(int col, int row, HashSet<(int, int)> visited)
    {
        if (col < 0 || col >= Singleton.GAMEWIDTH || row < 0 || row >= Singleton.GAMEHEIGHT)
            return;

        if (visited.Contains((col, row)) || Singleton.Instance.GameBoard[col, row] == null)
            return;

        visited.Add((col, row));

        // ตรวจหาทาง 6 ทิศทางใน Hex Grid
        MarkConnectedBubbles(col - 1, row, visited);
        MarkConnectedBubbles(col + 1, row, visited);
        MarkConnectedBubbles(col, row - 1, visited);
        MarkConnectedBubbles(col, row + 1, visited);
        if (Singleton.IsRowEven(row))
        {
            MarkConnectedBubbles(col - 1, row - 1, visited);
            MarkConnectedBubbles(col - 1, row + 1, visited);
        }
        else
        {
            MarkConnectedBubbles(col + 1, row - 1, visited);
            MarkConnectedBubbles(col + 1, row + 1, visited);
        }
    }

    private int CheckAndDestroyBubbles(int col, int row, BubbleColor targetColor, HashSet<(int, int)> visited = null)
    {
        if (visited == null)
            visited = new HashSet<(int, int)>();

        // ถ้าค่าพิกัดอยู่นอกขอบเขตของบอร์ดให้ return
        if (col < 0 || col >= Singleton.GAMEWIDTH || row < 0 || row >= Singleton.GAMEHEIGHT)
            return 0;

        // ถ้าช่องว่าง หรือ สีไม่ตรงกัน หรือถูกเยี่ยมชมแล้ว ให้ return
        Bubble bubble = Singleton.Instance.GameBoard[col, row];
        if (bubble == null || bubble.CurrentColor != targetColor || visited.Contains((col, row)))
            return 0;

        // เพิ่มตำแหน่งนี้เข้าไปใน Set เพื่อป้องกันการนับซ้ำ
        visited.Add((col, row));

        // ค้นหาทาง 6 ทิศทางใน Hex Grid และนับจำนวนที่พบ
        int count = 1;
        count += CheckAndDestroyBubbles(col - 1, row, targetColor, visited);
        count += CheckAndDestroyBubbles(col + 1, row, targetColor, visited);
        count += CheckAndDestroyBubbles(col, row - 1, targetColor, visited);
        count += CheckAndDestroyBubbles(col, row + 1, targetColor, visited);
        if (Singleton.IsRowEven(row))
        {
            count += CheckAndDestroyBubbles(col - 1, row - 1, targetColor, visited);
            count += CheckAndDestroyBubbles(col - 1, row + 1, targetColor, visited);
        }
        else
        {
            count += CheckAndDestroyBubbles(col + 1, row - 1, targetColor, visited);
            count += CheckAndDestroyBubbles(col + 1, row + 1, targetColor, visited);
        }

        return count;
    }

    private void FloodFillDestroy(int col, int row, BubbleColor targetColor)
    {
        // ถ้าค่าพิกัดอยู่นอกขอบเขตของบอร์ดให้ return
        if (col < 0 || col >= Singleton.GAMEWIDTH || row < 0 || row >= Singleton.GAMEHEIGHT)
            return;

        // ถ้าช่องว่าง หรือ สีไม่ตรงกัน ให้ return
        Bubble bubble = Singleton.Instance.GameBoard[col, row];
        if (bubble == null || bubble.CurrentColor != targetColor)
            return;

        // ลบ Bubble นี้ออกจากบอร์ด
        // ก่อนลบ Bubble ให้เรียก Trigger Event
        Vector2 bubbleCenter = new Vector2(
            bubble.Position.X + Singleton.TILESIZE / 2,
            bubble.Position.Y + Singleton.TILESIZE / 2
        );
        lastDestroyedPosition = bubble.Position;
        Singleton.Instance.GameBoard[col, row] = null;
        comboDestroyCount++;

        // วาด VFX ระเบิด
        Singleton.TriggerBubbleDestroyed(bubbleCenter);

        // ค้นหาทาง 6 ทิศทางใน Hex Grid
        FloodFillDestroy(col - 1, row, targetColor); // ซ้าย
        FloodFillDestroy(col + 1, row, targetColor); // ขวา
        FloodFillDestroy(col, row - 1, targetColor); // บน
        FloodFillDestroy(col, row + 1, targetColor); // ล่าง
        if (Singleton.IsRowEven(row))
        {
            FloodFillDestroy(col - 1, row - 1, targetColor); // ซ้ายบน
            FloodFillDestroy(col - 1, row + 1, targetColor); // ซ้ายล่าง
        }
        else
        {
            FloodFillDestroy(col + 1, row - 1, targetColor); // ขวาบน
            FloodFillDestroy(col + 1, row + 1, targetColor); // ขวาล่าง
        }
    }
}
