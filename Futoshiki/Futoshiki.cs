public class Futoshiki {
    private char[,] vConstraints, hConstraints;
    private int[,,] constraints;
    private int size, guesses;
    private bool[,,] forbidden;

    public int Guesses => guesses;

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
        InitConstraints();
    }

    private void InitConstraints() {
        constraints = new int[size, size, 4]; // 0=vc>, 1=vc<, 2=hc>, 3=hc<
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

    public Futoshiki(int size, Random random) {
        this.size = size;
        var grid = MakeLatinSquare(size, random);
        for (;;) {
            vConstraints = new char[size - 1, size];
            hConstraints = new char[size, size - 1];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size - 1; j++)
                    vConstraints[j, i] = hConstraints[i, j] = ' ';
            forbidden = new bool[size, size, size];
            InitConstraints();
            if (Solve()) throw new ApplicationException("How did we uniquely solve this with no hints?");
            int hints = 0;
            List<(int, int)> numberHints = new();
            while (hints < size * size) {
                // add hint
                if (random.Next(4) == 0) { // number hint
                    int v = random.Next(size), h = random.Next(size);
                    if (numberHints.Contains((v, h))) continue;
                    numberHints.Add((v, h));
                } else if (random.Next(2) == 0) { // vertical constraint
                    int v = random.Next(size - 1), h = random.Next(size);
                    if (vConstraints[v, h] != ' ') continue;
                    int nh = grid[v, h], nl = grid[v + 1, h];
                    vConstraints[v, h] = nh < nl ? '^' : 'v';
                } else { // horizontal constraint
                    int v = random.Next(size), h = random.Next(size - 1);
                    if (hConstraints[v, h] != ' ') continue;
                    int nl = grid[v, h], nr = grid[v, h + 1];
                    hConstraints[v, h] = nl < nr ? '<' : '>';
                }
                forbidden = new bool[size, size, size];
                InitConstraints();
                foreach ((int v, int h) in numberHints)
                    SetValue(v, h, grid[v, h]);
                if (Solve()) break;
                hints++;
            }
            if (IsSolved()) {
                forbidden = new bool[size, size, size];
                foreach ((int v, int h) in numberHints)
                    SetValue(v, h, grid[v, h]);
                break;
            }
            // failed to make a uniquely solvable puzzle, try again
        }
    }

    private static int[,] MakeLatinSquare(int size, Random random) {
        for (;;) {
            var grid = new int[size, size];
            for (int v = 0; v < size; v++)
                for (int h = 0; h < size; h++) {
                    var ns = Enumerable.Range(1, size).ToHashSet();
                    for (int i = 0; i < v; i++) ns.Remove(grid[i, h]);
                    for (int i = 0; i < h; i++) ns.Remove(grid[v, i]);
                    if (ns.Count == 0) break;
                    int n = ns.ToList()[random.Next(ns.Count)];
                    grid[v, h] = n;
                }
            if (grid[size - 1, size - 1] > 0) return grid;
        }
    }

    private Futoshiki(Futoshiki input) { // copy constructor
        size = input.size;
        vConstraints = new char[size - 1, size];
        hConstraints = new char[size, size - 1];
        forbidden = new bool[size, size, size];
        constraints = new int[size, size, 4];
        for (int v = 0; v < size; v++)
            for (int h = 0; h < size; h++) {
                if (v < size - 1)
                    vConstraints[v, h] = input.vConstraints[v, h];
                if (h < size - 1)
                    hConstraints[v, h] = input.hConstraints[v, h];
                for (int n = 0; n < size; n++)
                    forbidden[v, h, n] = input.forbidden[v, h, n];
                for (int k = 0; k < 4; k++)
                    constraints[v, h, k] = input.constraints[v, h, k];
            }
    }

    private bool SetValue(int v, int h, int n) {
        if (forbidden[v, h, n - 1])
            throw new ArgumentException($"SetValue: value {n} at ({v},{h}) is forbidden");
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

    private int GetMinValue(int v, int h) {
        for (int i = 1; i <= size; i++)
            if (!forbidden[v, h, i - 1])
                return i;
        return 0;
    }

    private int GetMaxValue(int v, int h) {
        for (int i = size; i >= 1; i--)
            if (!forbidden[v, h, i - 1])
                return i;
        return 0;
    }

    public bool Solve() {
        int solutions = 0;
        if (!Solve(ref solutions, true)) {
            forbidden = new bool[size, size, size];
            return false;
        }
        forbidden = new bool[size, size, size];
        guesses = 0;
        if (!Solve(ref solutions, false)) {
            forbidden = new bool[size, size, size];
            return false;
        }
        return IsSolved();
    }

    private bool Solve(ref int solutions, bool uniqueTest) {
        for (int pass = 0; pass < 100; pass++) {
            bool didSomething = false;
            for (int v = 0; v < size; v++)
                for (int h = 0; h < size; h++)
                    if (Solve(v, h))
                        didSomething = true;
            if (!didSomething) {
                if (!IsValid()) break; // Invalid state
                if (IsSolved()) {
                    if (!uniqueTest) break;
                    if (solutions > 1) return false; // multiple solutions found
                    solutions++;
                }
            nextGuess: // try to guess a value
                var (v, h, n) = NextGuess();
                guesses++;
                if (n == 0) break; // No more guesses
                Console.WriteLine($"Guessing {n} at ({v},{h})");
                var f = new Futoshiki(this);
                f.SetValue(v, h, n);
                if (!f.Solve(ref solutions, uniqueTest)) return false;
                if (f.IsSolved() && f.IsValid()) { // guess worked
                    var grid = new int[size, size];
                    for (int i = 0; i < size; i++)
                        for (int j = 0; j < size; j++) {
                            grid[i, j] = GetValue(i, j);
                            for (int k = 0; k < size; k++)
                                forbidden[i, j, k] = f.forbidden[i, j, k];
                        }
                    if (uniqueTest) {
                        if (solutions > 1) return false; // multiple solutions found
                        solutions++;
                        forbidden[v, h, n - 1] = true;
                        goto nextGuess;
                    }
                    break;
                }
                forbidden[v, h, n - 1] = true; // guess failed, don't guess n again
            }
        }
        return true;
    }

    private (int v, int h, int n) NextGuess() {
        for (int v = 0; v < size; v++)
            for (int h = 0; h < size; h++) {
                int min = GetMinValue(v, h), max = GetMaxValue(v, h);
                if (min != max && min > 0 && max > 0) return (v, h, min);
            }
        return (0, 0, 0);
    }

    private bool Solve(int v, int h) {
        bool didSomething = false;

        int n = GetValue(v, h);
        if (n > 0) {
            // prevent others in row/col from being this value
            for (int i = 0; i < size; i++) {
                if (i != h && !forbidden[v, i, n - 1]) {
                    didSomething = forbidden[v, i, n - 1] = true;
                    if (GetValue(v, i) != 0)
                        Console.WriteLine($"Discovered {GetValue(v, i)} at ({v},{i}) #1");
                }
                if (i != v && !forbidden[i, h, n - 1]) {
                    didSomething = forbidden[i, h, n - 1] = true;
                    if (GetValue(i, h) != 0)
                        Console.WriteLine($"Discovered {GetValue(i, h)} at ({i},{h}) #2");
                }
            }
            return didSomething;
        }

        // restrict to possible values based on constraints
        for (int c = 0; c < 4; c++)
            for (int i = 0; i < constraints[v, h, c]; i++)
                if (!forbidden[v, h, c % 2 != 0 ? size - 1 - i : i]) {
                    didSomething = forbidden[v, h, c % 2 != 0 ? size - 1 - i : i] = true;
                    if (GetValue(v, h) != 0)
                        Console.WriteLine($"Discovered {GetValue(v, h)} at ({v},{h}) #3");
                }

        // only one in row or column that can be this value
        for (int nn = 1; nn <= size; nn++)
            if (!forbidden[v, h, nn - 1])
                if (Enumerable.Range(0, size).All(i => i == h || forbidden[v, i, nn - 1]) ||
                    Enumerable.Range(0, size).All(i => i == v || forbidden[i, h, nn - 1]))
                    if (SetValue(v, h, nn)) {
                        didSomething = true;
                        Console.WriteLine($"Discovered {nn} at ({v},{h}) #4");
                    }

        // do some more clever things here
        return didSomething;
    }

    public bool IsSolved() {
        for (int v = 0; v < size; v++)
            for (int h = 0; h < size; h++)
                if (GetValue(v, h) == 0)
                    return false;
        return true;
    }

    public bool IsValid() {
        for (int v = 0; v < size; v++)
            for (int h = 0; h < size; h++) {
                if (GetMinValue(v, h) == 0) return false; // Invalid state: no possible values
                var n = GetValue(v, h);
                if (n == 0) continue;
                if (Enumerable.Range(0, size).Any(i => i != h && n == GetValue(v, i))) return false;
                if (Enumerable.Range(0, size).Any(i => i != v && n == GetValue(i, h))) return false;
                if (v > 0 && vConstraints[v - 1, h] == '^' && n <= GetMaxValue(v - 1, h)) return false;
                if (v > 0 && vConstraints[v - 1, h] == 'v' && n >= GetMinValue(v - 1, h)) return false;
                if (v < size - 1 && vConstraints[v, h] == '^' && n >= GetMaxValue(v + 1, h)) return false;
                if (v < size - 1 && vConstraints[v, h] == 'v' && n <= GetMinValue(v + 1, h)) return false;
                if (h > 0 && hConstraints[v, h - 1] == '<' && n <= GetMaxValue(v, h - 1)) return false;
                if (h > 0 && hConstraints[v, h - 1] == '>' && n >= GetMinValue(v, h - 1)) return false;
                if (h < size - 1 && hConstraints[v, h] == '<' && n >= GetMaxValue(v, h + 1)) return false;
                if (h < size - 1 && hConstraints[v, h] == '>' && n <= GetMinValue(v, h + 1)) return false;
            }
        return true;
    }

    public void Print() {
        for (int v = 0; v < size; v++) {
            for (int h = 0; h < size; h++) {
                int value = GetValue(v, h);
                Console.Write(value > 0 ? (char)(value + '0') : '?');
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
    }

    public void PrintDebug() {
        for (int v = 0; v < size; v++) {
            for (int n = 0; n < size; n++) {
                for (int h = 0; h < size; h++) {
                    Console.Write(forbidden[v, h, n] ? 'x' : (n + 1).ToString());
                    if (h < size - 1)
                        Console.Write(hConstraints[v, h]);
                }
                Console.Write("   ");
            }
            Console.WriteLine();
            if (v < size - 1) {
                for (int n = 0; n < size; n++) {
                    for (int h = 0; h < size; h++) {
                        Console.Write(vConstraints[v, h]);
                        if (h < size - 1)
                            Console.Write(" ");
                    }
                    Console.Write("   ");
                }
                Console.WriteLine();
            }
        }
    }

    public string Solution() {
        char[] result = new char[size * size + size - 1];
        for (int i = 0; i < result.Length; i++) result[i] = '\n';
        for (int v = 0; v < size; v++)
            for (int h = 0; h < size; h++) {
                int value = GetValue(v, h);
                result[v * (size + 1) + h] = value > 0 ? (char)(value + '0') : '?';
            }
        return new string(result);
    }
}