using System.Numerics;
using Raylib_cs;

class Program
{
    const float step = 5f;

    private static Genome[] genomes = new Genome[]
    {
        new Genome(1), new Genome(2),
        new Genome(3), new Genome(4),
        new Genome(5), new Genome(6),
        new Genome(7), new Genome(8)
    };

    public static void Main(string[] args)
    {
        Raylib.InitWindow(1300, 1000, "Hello world");
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                foreach (Genome genome in genomes)
                {
                    genome.Regenerate();
                }
            }

            Data data = genomes[^1].GetData();
            Draw(genomes.Length - 1, new Vector2(650, 800), MathF.PI / 2, data);

            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }

    private static void Draw(int generation, Vector2 pos, float angle, Data data)
    {
        Vector2 offset = new Vector2(1f, 1f);
        Vector2 norm = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        for (float i = 0; i < data.length; i += step)
        {
            Color col = new Color(data.r, data.g, data.b);

            Raylib.DrawCircleV(pos + offset, data.size, new Color(1f, 1f, 1f, 0.1f));
            Raylib.DrawCircleV(pos - offset, data.size, new Color(0f, 0f, 0f, 0.1f));
            Raylib.DrawCircleV(pos, data.size, col);

            pos.Y += data.gravity * i;
            pos -= norm * step;
            angle += data.turn_step;
            norm = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

            data.size *= data.size_step_scale;
            data.r *= data.r_step_scale;
            data.g *= data.g_step_scale;
            data.b *= data.b_step_scale;
        }

        if (generation == 0)
            return;

        Data next_data = genomes[generation - 1].GetData(data);

        float startAngle = data.branches_count == 2 ? -0.5f : -1f;
        startAngle *= data.brances_angle;
        for (int i = 0; i < data.branches_count; i++)
        {
            Draw(generation - 1, pos, angle + startAngle + i * data.brances_angle, next_data);
        }
    }
}

public struct Data
{
    public float length, size, size_step_scale, turn_step, gravity;
    public float branches_count;
    public float brances_angle;
    public float r, g, b, r_step_scale, g_step_scale, b_step_scale;
}

public class Genome
{

    public float
        length, length_deviation,
        size, size_deviation, size_from_ancestor,
        size_step_scale, turn_step, turn_step_deviation, gravity;

    public int branches_count;
    public float brances_angle, branches_angle_deviation;

    public float
        r, g, b,
        r_step_scale, g_step_scale, b_step_scale,
        color_from_ancestor, color_deviation;


    private const float MAX_GENERATIONS = 8f;
    private const float MAX_LEN = 120f;
    private const float MAX_SIZE = 6f;
    private const float MAX_DEVIATION_PERCENT = 0.05f;
    private const float STEP_SCALE_PERCENT = 0.01f;

    private int generation;

    private static Random rand = new();

    public Genome(int generation)
    {
        this.generation = generation;
        Regenerate();
    }

    public void Regenerate()
    {
        length = MathF.Pow(0.9f, MAX_GENERATIONS / generation) * MAX_LEN;
        length_deviation = GetDeviation();

        size = MathF.Pow(0.9f, MAX_GENERATIONS / generation) * MAX_SIZE;
        size_deviation = GetDeviation();
        size_from_ancestor = 0.6f + rand.NextSingle() * 0.4f;
        size_step_scale = 1f + (rand.Next(0, 2) * 2 - 1) * rand.NextSingle() * STEP_SCALE_PERCENT;

        turn_step = rand.NextSingle() * (Single.Lerp(1f, 3f, 1 - generation / MAX_GENERATIONS) / 180f * MathF.PI) * (rand.Next(0, 2) * 2 - 1);
        turn_step_deviation = GetDeviation();

        gravity = Random.Shared.NextSingle() * 0.02f;

        branches_count = rand.Next(2, 4);
        brances_angle = (0.8f + 0.2f * rand.NextSingle()) * (45f / 180f * MathF.PI);
        branches_angle_deviation = GetDeviation();

        r = 0.5f + 0.5f * rand.NextSingle();
        g = 0.5f + 0.5f * rand.NextSingle();
        b = 0.5f + 0.5f * rand.NextSingle();

        r_step_scale = 1f - rand.NextSingle() * STEP_SCALE_PERCENT;
        g_step_scale = 1f - rand.NextSingle() * STEP_SCALE_PERCENT;
        b_step_scale = 1f - rand.NextSingle() * STEP_SCALE_PERCENT;

        color_from_ancestor = 0.8f + rand.NextSingle() * 0.2f;
        color_deviation = GetDeviation();
    }

    public Data GetData()
    {
        Data result = new Data
        {
            length = this.length * (1f + this.length_deviation),
            size = this.size * (1f + this.size_deviation),
            size_step_scale = this.size_step_scale,
            turn_step = this.turn_step * (1f + this.turn_step_deviation),
            gravity = this.gravity,
            branches_count = this.branches_count,
            brances_angle = this.brances_angle * (1f + this.branches_angle_deviation),
            r = this.r * (1f + this.color_deviation),
            g = this.g * (1f + this.color_deviation),
            b = this.b * (1f + this.color_deviation),
            r_step_scale = this.r_step_scale,
            g_step_scale = this.g_step_scale,
            b_step_scale = this.b_step_scale
        };

        return result;
    }

    public Data GetData(Data parent)
    {

        Data result = new Data
        {
            length = this.length * (1f + this.length_deviation),

            size = Single.Lerp(this.size * (1f + this.size_deviation), parent.size, this.size_from_ancestor),
            size_step_scale = this.size_step_scale,

            turn_step = this.turn_step * (1f + this.turn_step_deviation),
            gravity = this.gravity,

            branches_count = this.branches_count,
            brances_angle = this.brances_angle * (1f + this.branches_angle_deviation),

            r = Single.Lerp(this.r * (1f + this.color_deviation), parent.r, this.color_from_ancestor),
            g = Single.Lerp(this.g * (1f + this.color_deviation), parent.g, this.color_from_ancestor),
            b = Single.Lerp(this.b * (1f + this.color_deviation), parent.b, this.color_from_ancestor),

            r_step_scale = this.r_step_scale,
            g_step_scale = this.g_step_scale,
            b_step_scale = this.b_step_scale
        };

        return result;
    }

    private float GetDeviation() => (rand.Next(0, 2) * 2 - 1) * rand.NextSingle() * MAX_DEVIATION_PERCENT;
}
