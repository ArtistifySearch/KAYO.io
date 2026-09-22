using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TowerDefense
{
    public class GameForm : Form
    {
        private Timer gameTimer = new Timer();
        private List<Enemy> enemies = new List<Enemy>();
        private List<Tower> towers = new List<Tower>();
        private List<Projectile> projectiles = new List<Projectile>();
        private Point[] pathPoints = new Point[]
        {
            new Point(0, 100),
            new Point(300, 100),
            new Point(300, 300),
            new Point(600, 300)
        };

        public GameForm()
        {
            this.Text = "Top-Down Tower Defense";
            this.Size = new Size(800, 400);
            this.DoubleBuffered = true;

            this.MouseClick += GameForm_MouseClick;

            gameTimer.Interval = 30;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            SpawnEnemy();
        }

        private void GameForm_MouseClick(object sender, MouseEventArgs e)
        {
            // Place towers on right click
            if (e.Button == MouseButtons.Right)
            {
                towers.Add(new Tower(e.Location));
            }
        }

        private void SpawnEnemy()
        {
            enemies.Add(new Enemy(pathPoints));
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Move enemies
            foreach (var enemy in enemies)
            {
                enemy.MoveAlongPath();
            }

            // Towers attack enemies
            foreach (var tower in towers)
            {
                Enemy target = null;
                double minDist = double.MaxValue;
                foreach (var enemy in enemies)
                {
                    double dist = tower.Position.Distance(enemy.Position);
                    if (dist < minDist && dist <= tower.Range)
                    {
                        minDist = dist;
                        target = enemy;
                    }
                }
                if (target != null)
                {
                    // Fire projectile
                    projectiles.Add(new Projectile(tower.Position, target));
                }
            }

            // Move projectiles
            foreach (var proj in projectiles)
            {
                proj.Move();
            }

            // Check for collisions
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                for (int j = projectiles.Count - 1; j >= 0; j--)
                {
                    Projectile p = projectiles[j];
                    if (p.Target == enemy && p.Position.Distance(enemy.Position) < 5)
                    {
                        enemies.RemoveAt(i);
                        projectiles.RemoveAt(j);
                        break;
                    }
                }
            }

            // Remove enemies that reach the end
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].HasReachedEnd(pathPoints))
                {
                    enemies.RemoveAt(i);
                }
            }

            // Spawn new enemies periodically
            // For simplicity, spawn every 100 frames
            if (Environment.TickCount % 100 == 0)
            {
                SpawnEnemy();
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // Draw path
            g.DrawLines(Pens.Gray, pathPoints);

            // Draw enemies
            foreach (var enemy in enemies)
            {
                g.FillEllipse(Brushes.Red, enemy.Position.X - 5, enemy.Position.Y - 5, 10, 10);
            }

            // Draw towers
            foreach (var tower in towers)
            {
                g.FillRectangle(Brushes.Blue, tower.Position.X - 10, tower.Position.Y - 10, 20, 20);
            }

            // Draw projectiles
            foreach (var p in projectiles)
            {
                g.FillEllipse(Brushes.Black, p.Position.X - 3, p.Position.Y - 3, 6, 6);
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new GameForm());
        }
    }

    public class Enemy
    {
        public PointF Position;
        private List<Point> pathPoints;
        private int currentTargetIndex = 1;
        private float speed = 1.5f;

        public Enemy(Point[] path)
        {
            Position = new PointF(path[0].X, path[0].Y);
            pathPoints = new List<Point>(path);
        }

        public void MoveAlongPath()
        {
            if (currentTargetIndex >= pathPoints.Count) return;

            Point target = pathPoints[currentTargetIndex];
            float dx = target.X - Position.X;
            float dy = target.Y - Position.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1)
            {
                currentTargetIndex++;
            }
            else
            {
                Position.X += dx / dist * speed;
                Position.Y += dy / dist * speed;
            }
        }

        public bool HasReachedEnd(Point[] path)
        {
            return currentTargetIndex >= path.Length;
        }
    }

    public class Tower
    {
        public PointF Position;
        public float Range = 100f;

        public Tower(Point location)
        {
            Position = new PointF(location.X, location.Y);
        }
    }

    public class Projectile
    {
        public PointF Position;
        public Enemy Target;
        private float speed = 4f;

        public Projectile(PointF start, Enemy target)
        {
            Position = new PointF(start.X, start.Y);
            Target = target;
        }

        public void Move()
        {
            if (Target == null) return;
            float dx = Target.Position.X - Position.X;
            float dy = Target.Position.Y - Position.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1) return;

            Position.X += dx / dist * speed;
            Position.Y += dy / dist * speed;
        }
    }

    public static class Extensions
    {
        public static double Distance(this PointF p1, PointF p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
