using System;
using System.IO;
using System.Text;
using System.Numerics;
using Raylib_cs;

namespace FlappyBird
{
    class Program
    {
        static void Main(string[] args)
        {
            Raylib.InitWindow(800, 450, "Flappy Bird");
            Raylib.SetTargetFPS(60);
            Raylib.SetExitKey(KeyboardKey.Null);

            Raylib.InitAudioDevice();
            Sound gameOverSound = Raylib.LoadSound("gameOver.mp3");
            Sound scoreSound = Raylib.LoadSound("scoreEarningSound.mp3");

            // 1. Initialize the Random number generator
            Random random = new Random();

            // Bird variables
            float birdY = 225.0f;
            float birdVelocity = 0.0f;
            const float birdRadius = 16.0f;

            // Floor variables
            float floorX = 0.0f;
            const float floorY = 400.0f; 

            // === PIPE CONFIGURATION (REAL FLAPPY BIRD STYLE) ===
            const float pipeWidth = 60.0f;
            const float gapSize = 130.0f; // Safe gap size for the bird to fly through
            const float distanceBetweenPipes = 430.0f; // Ideal distance between consecutive pipes

             // === SPEED VARIABLES ===
            const float baseSpeed = 3.0f;      // The starting speed of the game
            float gameSpeed = baseSpeed;       // The current active speed that changes with score
            // =======================

            // Pipe 1 setup
            float pipe1X = 800.0f;       
            float pipe1TopHeight = (float)random.Next(40, (int)(floorY - gapSize - 40));
            bool pipe1Passed = false;

            // Pipe 2 setup (Starts farther ahead to keep the distance short and steady)
            float pipe2X = 800.0f + distanceBetweenPipes;       
            float pipe2TopHeight = (float)random.Next(40, (int)(floorY - gapSize - 40));
            bool pipe2Passed = false;
            // ===================================================

            int gameState = 0;
            int score = 0;

            // === NEW: HIGH SCORE SYSTEM VARIABLES ===
            int highScore = 0;
            string highScoreFile = "highscore.txt";
            bool isGameOverSaved = false;

            // Load the high score from the file if it exists
            if (File.Exists(highScoreFile))
            {
                string savedScore = File.ReadAllText(highScoreFile);
                int.TryParse(savedScore, out highScore);
            }
            // ========================================

            while (!Raylib.WindowShouldClose())
            {
                if (gameState == 0)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Space))
                    {
                        gameState = 1;
                    }
                }

                else if (gameState == 1)        
                {
                    // Bird Input & Physics
                    if (Raylib.IsKeyPressed(KeyboardKey.Space))
                    {
                        birdVelocity = -7.0f;
                    }
                    birdVelocity = birdVelocity + 0.4f; 
                    birdY = birdY + birdVelocity;

                    // Move floor based on dynamic speed
                    floorX = floorX - gameSpeed;
                    if (floorX <= -40.0f) floorX = 0.0f; 

                    // Move pipes based on dynamic speed
                    pipe1X = pipe1X - gameSpeed;
                    pipe2X = pipe2X - gameSpeed;
                    
                    // Recycle Pipe 1
                    if (pipe1X < -60.0f)
                    {
                        pipe1X = pipe2X + distanceBetweenPipes; // Place behind pipe 2 keeping the gap distance
                        pipe1TopHeight = (float)random.Next(40, (int)(floorY - gapSize - 40));
                        pipe1Passed = false;
                    }

                    // Recycle Pipe 2
                    if (pipe2X < -60.0f)
                    {
                        pipe2X = pipe1X + distanceBetweenPipes; // Place behind pipe 1 keeping the gap distance
                        pipe2TopHeight = (float)random.Next(40, (int)(floorY - gapSize - 40));
                        pipe2Passed = false;
                    }
                    // =======================

                    // === INDEPENDENT SCORE SYSTEM ===
                    if ((pipe1Passed == false) && ((pipe1X + pipeWidth) < 100.0f))
                    {
                        score = score + 1;
                        pipe1Passed = true;
                        gameSpeed += 0.2f;
                        Raylib.PlaySound(scoreSound);

                        if (score > highScore)
                        {
                            highScore = score;
                        }
                    }
                    if ((pipe2Passed == false) && ((pipe2X + pipeWidth) < 100.0f))
                    {
                        score = score + 1;
                        pipe2Passed = true;
                        gameSpeed += 0.2f;
                        Raylib.PlaySound(scoreSound);

                        // Update high score immediately in memory if beaten
                        if (score > highScore)
                        {
                            highScore = score;
                        }
                    }
                    // ================================
                    
                    // Floor collision
                    if (birdY + birdRadius >= floorY) 
                    {
                        birdY = floorY - birdRadius; 
                        Raylib.PlaySound(gameOverSound);
                        gameState = 2;          
                    }
                    
                    // === COLLISION CHECK FOR BOTH PIPES ===
                    Vector2 birdCenter = new Vector2(100.0f, birdY);

                    // Pipe 1 bounding rectangles
                    Rectangle topPipe1 = new Rectangle(pipe1X, 0.0f, pipeWidth, pipe1TopHeight);
                    float bottomPipe1Y = pipe1TopHeight + gapSize;
                    Rectangle bottomPipe1 = new Rectangle(pipe1X, bottomPipe1Y, pipeWidth, floorY - bottomPipe1Y);

                    // Pipe 2 bounding rectangles
                    Rectangle topPipe2 = new Rectangle(pipe2X, 0.0f, pipeWidth, pipe2TopHeight);
                    float bottomPipe2Y = pipe2TopHeight + gapSize;
                    Rectangle bottomPipe2 = new Rectangle(pipe2X, bottomPipe2Y, pipeWidth, floorY - bottomPipe2Y);

                    if (Raylib.CheckCollisionCircleRec(birdCenter, birdRadius, topPipe1) ||
                        Raylib.CheckCollisionCircleRec(birdCenter, birdRadius, bottomPipe1) ||
                        Raylib.CheckCollisionCircleRec(birdCenter, birdRadius, topPipe2) ||
                        Raylib.CheckCollisionCircleRec(birdCenter, birdRadius, bottomPipe2))
                    {
                        Raylib.PlaySound(gameOverSound);
                        gameState = 2; 
                    }
                    // ======================================

                    // Ceiling limit
                    if (birdY < 0.0f)
                    {
                        birdY = 0.0f;
                        birdVelocity = 0.0f;
                    }
                } 
                else if (gameState == 2)
                {
                    // === NEW: SAVE FILE STRICTLY ONCE WHEN GAME OVER STATE RUNS ===
                    if (isGameOverSaved == false)
                    {
                        File.WriteAllText(highScoreFile, highScore.ToString());
                        isGameOverSaved = true; // Locks the saving trigger until the player restarts
                    }
                    // ==============================================================

                    if (Raylib.IsKeyPressed(KeyboardKey.Space))
                    {
                        birdY = 225.0f;
                        birdVelocity = 0.0f;
                        floorX = 0.0f;

                        // Reset dynamic speed for a fresh start
                        gameSpeed = baseSpeed;
                        
                        // Reset both pipes keeping the proper distance layout
                        pipe1X = 800.0f;
                        pipe1TopHeight = (float)random.Next(40, (int)(floorY - gapSize - 40));
                        pipe1Passed = false;

                        pipe2X = 800.0f + distanceBetweenPipes;
                        pipe2TopHeight = (float)random.Next(40, (int)(floorY - gapSize - 40));
                        pipe2Passed = false;
                        
                        // Point reset
                        score = 0;
                        isGameOverSaved = false;
                        gameState = 1;
                    }
                }

                // --- RENDER ---
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Blue);

                // Draw Bird
                Raylib.DrawCircle(100, (int)birdY, (int)birdRadius, Color.Yellow);

                // === RENDER PIPE 1 ===
                Raylib.DrawRectangle((int)pipe1X, 0, (int)pipeWidth, (int)pipe1TopHeight, Color.Lime);
                float bPipe1YDraw = pipe1TopHeight + gapSize;
                Raylib.DrawRectangle((int)pipe1X, (int)bPipe1YDraw, (int)pipeWidth, (int)(floorY - bPipe1YDraw), Color.Lime);

                // === RENDER PIPE 2 ===
                Raylib.DrawRectangle((int)pipe2X, 0, (int)pipeWidth, (int)pipe2TopHeight, Color.Lime);
                float bPipe2YDraw = pipe2TopHeight + gapSize;
                Raylib.DrawRectangle((int)pipe2X, (int)bPipe2YDraw, (int)pipeWidth, (int)(floorY - bPipe2YDraw), Color.Lime);

                // Draw Floor
                Raylib.DrawRectangle((int)floorX, (int)floorY, 840, 50, Color.DarkGray);
                Raylib.DrawRectangle((int)floorX, (int)floorY, 840, 10, Color.Lime);
                
                // UI Text
                if (gameState == 1 || gameState == 2)
                {
                    Raylib.DrawText("SCORE: " + score, 20, 20, 30, Color.White);
                    Raylib.DrawText("HI-SCORE: " + highScore, 580, 20, 30, Color.Gold);
                }

                if (gameState == 0)
                {
                    Raylib.DrawText("PRESS SPACE TO START", 250, 180, 24, Color.White);
                }
                else if (gameState == 2)
                {
                    Raylib.DrawText("GAME OVER - PRESS SPACE TO RESTART", 160, 180, 24, Color.Red);
                }

                Raylib.EndDrawing();
            }
            Raylib.UnloadSound(gameOverSound);
            Raylib.UnloadSound(scoreSound);
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
        }
    }
}
