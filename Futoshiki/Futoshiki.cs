public class Futoshiki {
    private char[,] vConstraints, hConstraints;
    private char[,,] constraints;
    private int size;
    private bool[,,] forbidden;

    public Futoshiki(string input) {
        var ss = input.Split('\n');
        if (ss.Length % 2 == 0)
            throw new ArgumentException("Invalid input: expected odd number of lines");
        size = ss.Length / 2 + 1;
        vConstraints = new char[size - 1, size];
        hConstraints = new char[size, size - 1];
        forbidden = new bool[size, size, size]; // forbidden[v,h,n] = true if n+1 is forbidden
        for (int v = 0; v < ss.Length; v++) {
            if (ss[v].Length != ss.Length)
                throw new ArgumentException("Invalid input: expected square grid");
            if (v % 2 == 0) {
                for (int h = 0; h < ss.Length; h += 2)
                    if (ss[v][h] != '?') {
                        if (!char.IsDigit(ss[v][h]) || ss[v][h] - '0' > size || ss[v][h] - '0' <= 0)
                            throw new ArgumentException("Invalid input: expected digit between 1 and " + size + " or ?");
                        SetValue(v / 2, h / 2, ss[v][h] - '0');
                    }
                for (int h = 1; h < ss.Length; h += 2)
                    if (!">< ".Contains(ss[v][h])) throw new ArgumentException("Invalid input: expected >, < or space");
                    else hConstraints[v / 2, h / 2] = ss[v][h];
            } else {
                for (int h = 0; h < ss.Length; h += 2)
                    if (!"^v ".Contains(ss[v][h])) throw new ArgumentException("Invalid input: expected ^, v or space");
                    else vConstraints[v / 2, h / 2] = ss[v][h];
                for (int h = 1; h < ss.Length; h += 2)
                    if (ss[v][h] != ' ')
                        throw new ArgumentException("Invalid input: expected space");
            }
        }
        constraints = new char[size, size, 4]; // 0=vc>, 1=vc<, 2=hc>, 3=hc<
        // convert vConstraints and hConstraints to constraints to make solving easier
        for (int v = 0; v < size; v++)
            for (int h = 0; h < size; h++) {
                if (v > 0 && vConstraints[v - 1, h] != ' ')
                    constraints[v, h, vConstraints[v - 1, h] == '^' ? 0 : 1]++;
                if (v < size - 1 && vConstraints[v, h] != ' ')
                    constraints[v, h, vConstraints[v, h] == '^' ? 1 : 0]++;
                if (h > 0 && hConstraints[v, h - 1] != ' ')
                    constraints[v, h, hConstraints[v, h - 1] == '<' ? 2 : 3]++;
                if (h < size - 1 && hConstraints[v, h] != ' ')
                    constraints[v, h, hConstraints[v, h] == '<' ? 3 : 2]++;
            }
    }

    private bool SetValue(int v, int h, int n) {
        if (forbidden[v, h, n - 1])
            throw new ArgumentException("SetValue: value " + n + " at (" + v + "," + h + ") is forbidden");
        bool didSomething = false;
        for (int i = 1; i <= size; i++)
            if (i != n && !forbidden[v, h, i - 1])
                didSomething = forbidden[v, h, i - 1] = true;
        return didSomething;
    }

    private int GetValue(int v, int h) {
        int value = 0;
        for (int i = 1; i <= size; i++)
            if (!forbidden[v, h, i - 1]) {
                if (value > 0) return 0;
                value = i;
            }
        return value;
    }

    public void Solve() {
        for (int pass = 0; pass < 100; pass++) {
            Console.WriteLine($"Pass {pass + 1}:");
            bool didSomething = false;
            for (int v = 0; v < size; v++)
                for (int h = 0; h < size; h++)
                    didSomething |= Solve(v, h);
            if (!didSomething) break;
        }
    }

    private bool Solve(int v, int h) {
        if (GetValue(v, h) > 0) return false;
        bool didSomething = false;

        // restrict to possible values based on constraints
        for (int c = 0; c < 4; c++)
            for (int i = 0; i < constraints[v, h, c]; i++)
                if (!forbidden[v, h, c % 2 != 0 ? size - 1 - i : i])
                    didSomething = forbidden[v, h, c % 2 != 0 ? size - 1 - i : i] = true;

        // can't be the same as any other value in the same row or column
        for (int i = 0; i < size; i++) {
            int nv = GetValue(i, h), nh = GetValue(v, i);
            if (i != v && nv != 0 && !forbidden[i, h, nv - 1]) didSomething = forbidden[i, h, nv - 1] = true;
            if (i != h && nh != 0 && !forbidden[v, i, nh - 1]) didSomething = forbidden[v, i, nh - 1] = true;
        }

        // only one in row or column that can be this value
        for (int n = 1; n <= size; n++) {
            if (forbidden[v, h, n - 1]) continue;
            var onlyInThisRow = Enumerable.Range(0, size).All(i => i == h || forbidden[v, i, n - 1]);
            var onlyInThisCol = Enumerable.Range(0, size).All(i => i == v || forbidden[i, h, n - 1]);
            if (onlyInThisRow || onlyInThisCol) didSomething |= SetValue(v, h, n);
        }

        // do some more clever things here
        return didSomething;
    }

    public bool Print() {
        bool isSolved = true;

        for (int v = 0; v < size; v++) {
            for (int h = 0; h < size; h++) {
                int value = GetValue(v, h);
                Console.Write(value > 0 ? (char)(value + '0') : '?');
                isSolved &= value > 0;
                if (h < size - 1)
                    Console.Write(hConstraints[v, h]);
            }
            Console.WriteLine();
            if (v < size - 1) {
                for (int h = 0; h < size; h++) {
                    Console.Write(vConstraints[v, h]);
                    if (h < size - 1)
                        Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        return isSolved;
    }
}