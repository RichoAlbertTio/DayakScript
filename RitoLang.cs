using System;
using System.Collections.Generic;
using System.Globalization;

public class RitoLang
{
    private readonly Action<string> _output;
    public RitoLang(Action<string> output) => _output = output;

    // ===== Value (angka / string) =====
    enum VKind { Num, Str }
    struct Value
    {
        public VKind Kind;
        public double Num;
        public string Str;

        public static Value N(double d) => new Value { Kind = VKind.Num, Num = d, Str = "" };
        public static Value S(string s) => new Value { Kind = VKind.Str, Str = s ?? "", Num = 0 };

        public override string ToString()
            => Kind == VKind.Str ? Str : Num.ToString(CultureInfo.InvariantCulture);
    }

    // ===== TOKENS =====
    enum TokenType
    {
        EOF, Identifier, Number, String,
        Let, Print, If, Else, True, False,
        Plus, Minus, Star, Slash,
        Equal, Semicolon, LParen, RParen, LBrace, RBrace,
        EqualEqual, BangEqual, Less, LessEqual, Greater, GreaterEqual
    }

    struct Token
    {
        public TokenType Type;
        public string Lexeme; // juga dipakai utk isi string literal
        public double Number; // hanya utk Number
        public int Pos;
        public Token(TokenType t, string lex, int pos, double num = 0)
        { Type = t; Lexeme = lex; Pos = pos; Number = num; }
    }

    // ===== LEXER =====
    class Lexer
    {
        private readonly string _src;
        private int _i;
        private readonly int _n;
        private readonly Dictionary<string, TokenType> _kw;

        public Lexer(string src, Dictionary<string, TokenType> keywordMap)
        { _src = src; _n = src.Length; _i = 0; _kw = keywordMap; }

        private char Peek(int k = 0) => (_i + k) < _n ? _src[_i + k] : '\0';
        private char Advance() => _i < _n ? _src[_i++] : '\0';

        private void SkipWS()
        {
            while (true)
            {
                char c = Peek();
                if (char.IsWhiteSpace(c)) { Advance(); continue; }
                // komentar // ... sampai akhir baris
                if (c == '/' && Peek(1) == '/')
                {
                    while (Peek() != '\n' && Peek() != '\0') Advance();
                    continue;
                }
                break;
            }
        }

        public List<Token> Scan()
        {
            var list = new List<Token>();
            while (true)
            {
                SkipWS();
                int pos = _i;
                char c = Peek();
                if (c == '\0') { list.Add(new Token(TokenType.EOF, "", pos)); break; }

                if (char.IsLetter(c) || c == '_')
                {
                    string id = ReadIdent();
                    string low = id.ToLowerInvariant();
                    if (_kw.TryGetValue(low, out var t)) list.Add(new Token(t, id, pos));
                    else list.Add(new Token(TokenType.Identifier, id, pos));
                }
                else if (char.IsDigit(c))
                {
                    list.Add(ReadNumber());
                }
                else
                {
                    switch (Advance())
                    {
                        case '+': list.Add(new Token(TokenType.Plus, "+", pos)); break;
                        case '-': list.Add(new Token(TokenType.Minus, "-", pos)); break;
                        case '*': list.Add(new Token(TokenType.Star, "*", pos)); break;
                        case '/': list.Add(new Token(TokenType.Slash, "/", pos)); break;
                        case '=':
                            if (Peek() == '=') { Advance(); list.Add(new Token(TokenType.EqualEqual, "==", pos)); }
                            else list.Add(new Token(TokenType.Equal, "=", pos));
                            break;
                        case '!':
                            if (Peek() == '=') { Advance(); list.Add(new Token(TokenType.BangEqual, "!=", pos)); }
                            else throw new Exception($"'!' hanya valid sebagai '!=' di {pos}");
                            break;
                        case '<':
                            if (Peek() == '=') { Advance(); list.Add(new Token(TokenType.LessEqual, "<=", pos)); }
                            else list.Add(new Token(TokenType.Less, "<", pos));
                            break;
                        case '>':
                            if (Peek() == '=') { Advance(); list.Add(new Token(TokenType.GreaterEqual, ">=", pos)); }
                            else list.Add(new Token(TokenType.Greater, ">", pos));
                            break;
                        case ';': list.Add(new Token(TokenType.Semicolon, ";", pos)); break;
                        case '(': list.Add(new Token(TokenType.LParen, "(", pos)); break;
                        case ')': list.Add(new Token(TokenType.RParen, ") ", pos)); break;
                        case '{': list.Add(new Token(TokenType.LBrace, "{", pos)); break;
                        case '}': list.Add(new Token(TokenType.RBrace, "}", pos)); break;
                        case '"': list.Add(ReadString(pos)); break;
                        default: throw new Exception($"Karakter tak dikenal di {pos}");
                    }
                }
            }
            return list;
        }

        private string ReadIdent()
        {
            int start = _i;
            while (char.IsLetterOrDigit(Peek()) || Peek() == '_') Advance();
            return _src.Substring(start, _i - start);
        }

        private Token ReadNumber()
        {
            int start = _i;
            while (char.IsDigit(Peek())) Advance();
            if (Peek() == '.')
            {
                Advance();
                while (char.IsDigit(Peek())) Advance();
            }
            string lex = _src.Substring(start, _i - start);
            double val = double.Parse(lex, CultureInfo.InvariantCulture);
            return new Token(TokenType.Number, lex, start, val);
        }

        private Token ReadString(int pos)
        {
            // posisi saat ini: sudah melewati tanda kutip pembuka
            var buf = new System.Text.StringBuilder();
            while (true)
            {
                char c = Advance();
                if (c == '\0') throw new Exception($"String belum ditutup di {pos}");
                if (c == '"') break; // selesai
                if (c == '\\')
                {
                    char n = Advance();
                    switch (n)
                    {
                        case '"': buf.Append('"'); break;
                        case '\\': buf.Append('\\'); break;
                        case 'n': buf.Append('\n'); break;
                        case 't': buf.Append('\t'); break;
                        case 'r': buf.Append('\r'); break;
                        default: buf.Append(n); break; // escape tak dikenal → literal saja
                    }
                }
                else
                {
                    buf.Append(c);
                }
            }
            return new Token(TokenType.String, buf.ToString(), pos);
        }
    }

    // ===== AST =====
    abstract class Stmt { }
    class LetStmt : Stmt { public string Name; public Expr Value; public LetStmt(string n, Expr v) { Name = n; Value = v; } }
    class PrintStmt : Stmt { public Expr Value; public PrintStmt(Expr v) { Value = v; } }
    class BlockStmt : Stmt { public List<Stmt> Body; public BlockStmt(List<Stmt> b) { Body = b; } }
    class IfStmt : Stmt
    {
        public Expr Cond;
        public Stmt Then;
        public Stmt? Else;
        public IfStmt(Expr c, Stmt t, Stmt? e) { Cond = c; Then = t; Else = e; }
    }

    abstract class Expr { }
    class Binary : Expr { public Expr L; public Token Op; public Expr R; public Binary(Expr l, Token op, Expr r) { L = l; Op = op; R = r; } }
    class Grouping : Expr { public Expr Inner; public Grouping(Expr e) { Inner = e; } }
    class Literal : Expr { public Value Val; public Literal(Value v) { Val = v; } }
    class Variable : Expr { public string Name; public Variable(string n) { Name = n; } }

    // ===== PARSER =====
    class Parser
    {
        private readonly List<Token> _toks;
        private int _i = 0;
        public Parser(List<Token> toks) { _toks = toks; }

        private Token Peek() => _toks[_i];
        private Token Advance() => _toks[_i++];
        private bool Check(TokenType t) => Peek().Type == t;

        private bool Match(params TokenType[] types)
        {
            foreach (var t in types)
                if (Check(t)) { Advance(); return true; }
            return false;
        }

        private Token Consume(TokenType t, string msg)
        {
            if (Check(t)) return Advance();
            throw new Exception($"{msg} (pos {_toks[_i].Pos})");
        }

        public List<Stmt> ParseProgram()
        {
            var stmts = new List<Stmt>();
            while (!Check(TokenType.EOF)) stmts.Add(Statement());
            return stmts;
        }

        private Stmt Statement()
        {
            if (Match(TokenType.Let))
            {
                var nameTok = Consume(TokenType.Identifier, "Harus ada nama variabel setelah 'let/biar/jituh'");
                Consume(TokenType.Equal, "Harus ada '=' setelah nama variabel");
                var expr = Expression();
                Consume(TokenType.Semicolon, "Harus diakhiri ';'");
                return new LetStmt(nameTok.Lexeme, expr);
            }
            if (Match(TokenType.Print))
            {
                var expr = Expression();
                Consume(TokenType.Semicolon, "Harus diakhiri ';'");
                return new PrintStmt(expr);
            }
            if (Match(TokenType.If))
            {
                Consume(TokenType.LParen, "Butuh '(' setelah if/misal");
                var cond = Expression();
                Consume(TokenType.RParen, "Butuh ')' setelah kondisi");
                var thenStmt = BlockOrSingle();
                Stmt? elseStmt = null;
                if (Match(TokenType.Else)) elseStmt = BlockOrSingle();
                return new IfStmt(cond, thenStmt, elseStmt);
            }
            if (Match(TokenType.LBrace))
            {
                var body = new List<Stmt>();
                while (!Check(TokenType.RBrace)) body.Add(Statement());
                Consume(TokenType.RBrace, "Butuh '}' di akhir blok");
                return new BlockStmt(body);
            }
            throw new Exception($"Statement tidak dikenal di pos {_toks[_i].Pos}");
        }

        private Stmt BlockOrSingle()
        {
            if (Match(TokenType.LBrace))
            {
                var body = new List<Stmt>();
                while (!Check(TokenType.RBrace)) body.Add(Statement());
                Consume(TokenType.RBrace, "Butuh '}' di akhir blok");
                return new BlockStmt(body);
            }
            return Statement();
        }

        // precedence: equality > comparison > term > factor > unary > primary
        private Expr Expression() => Equality();
        private Expr Equality()
        {
            var expr = Comparison();
            while (Match(TokenType.EqualEqual, TokenType.BangEqual))
            {
                var op = _toks[_i - 1];
                var right = Comparison();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr Comparison()
        {
            var expr = Term();
            while (Match(TokenType.Less, TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual))
            {
                var op = _toks[_i - 1];
                var right = Term();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr Term()
        {
            var expr = Factor();
            while (Match(TokenType.Plus, TokenType.Minus))
            {
                var op = _toks[_i - 1];
                var right = Factor();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr Factor()
        {
            var expr = Unary();
            while (Match(TokenType.Star, TokenType.Slash))
            {
                var op = _toks[_i - 1];
                var right = Unary();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr Unary() => Primary();

        private Expr Primary()
        {
            var t = Advance();
            switch (t.Type)
            {
                case TokenType.Number: return new Literal(Value.N(t.Number));
                case TokenType.String: return new Literal(Value.S(t.Lexeme));
                case TokenType.Identifier: return new Variable(t.Lexeme);
                case TokenType.True: return new Literal(Value.N(1));
                case TokenType.False: return new Literal(Value.N(0));
                case TokenType.LParen:
                    var inner = Expression();
                    Consume(TokenType.RParen, "Kurung tutup ')' hilang");
                    return new Grouping(inner);
                default:
                    throw new Exception($"Ekspresi tidak valid di pos {t.Pos}");
            }
        }
    }

    // ===== INTERPRETER =====
    class Interpreter
    {
        private readonly Action<string> _out;
        private readonly Dictionary<string, Value> _env = new Dictionary<string, Value>(StringComparer.Ordinal);

        public Interpreter(Action<string> o) { _out = o; }

        public void Exec(List<Stmt> stmts)
        {
            foreach (var s in stmts) Execute(s);
        }

        private void Execute(Stmt s)
        {
            switch (s)
            {
                case LetStmt l:
                    _env[l.Name] = Eval(l.Value);
                    break;
                case PrintStmt p:
                    _out(Eval(p.Value).ToString());
                    break;
                case BlockStmt b:
                    foreach (var st in b.Body) Execute(st);
                    break;
                case IfStmt iff:
                    var cond = Eval(iff.Cond);
                    if (IsTruthy(cond)) Execute(iff.Then);
                    else if (iff.Else != null) Execute(iff.Else);
                    break;
                default:
                    throw new Exception("Stmt tidak didukung");
            }
        }

        private static bool IsTruthy(Value v)
            => v.Kind == VKind.Num ? Math.Abs(v.Num) > double.Epsilon : !string.IsNullOrEmpty(v.Str);

        private static double AsNum(Value v)
        {
            if (v.Kind != VKind.Num) throw new Exception("Butuh angka di operasi aritmatika/kondisi numerik");
            return v.Num;
        }

        private Value Eval(Expr e)
        {
            switch (e)
            {
                case Literal lit: return lit.Val;
                case Grouping g: return Eval(g.Inner);
                case Variable v:
                    if (!_env.TryGetValue(v.Name, out var val))
                        throw new Exception($"Variabel '{v.Name}' belum dideklarasikan");
                    return val;
                case Binary b:
                    var L = Eval(b.L);
                    var R = Eval(b.R);
                    switch (b.Op.Type)
                    {
                        case TokenType.Plus:
                            if (L.Kind == VKind.Str || R.Kind == VKind.Str)
                                return Value.S(L.ToString() + R.ToString());
                            return Value.N(AsNum(L) + AsNum(R));
                        case TokenType.Minus: return Value.N(AsNum(L) - AsNum(R));
                        case TokenType.Star:  return Value.N(AsNum(L) * AsNum(R));
                        case TokenType.Slash:
                            var r = AsNum(R);
                            if (Math.Abs(r) < double.Epsilon) throw new DivideByZeroException();
                            return Value.N(AsNum(L) / r);

                        case TokenType.EqualEqual:
                            if (L.Kind == VKind.Num && R.Kind == VKind.Num)
                                return Value.N(Math.Abs(L.Num - R.Num) < 1e-9 ? 1 : 0);
                            if (L.Kind == VKind.Str && R.Kind == VKind.Str)
                                return Value.N(string.Equals(L.Str, R.Str, StringComparison.Ordinal) ? 1 : 0);
                            return Value.N(0); // tipe beda → tidak sama

                        case TokenType.BangEqual:
                            if (L.Kind == VKind.Num && R.Kind == VKind.Num)
                                return Value.N(Math.Abs(L.Num - R.Num) >= 1e-9 ? 1 : 0);
                            if (L.Kind == VKind.Str && R.Kind == VKind.Str)
                                return Value.N(string.Equals(L.Str, R.Str, StringComparison.Ordinal) ? 0 : 1);
                            return Value.N(1); // tipe beda → dianggap tidak sama

                        case TokenType.Less:         return Value.N(AsNum(L) <  AsNum(R) ? 1 : 0);
                        case TokenType.LessEqual:    return Value.N(AsNum(L) <= AsNum(R) ? 1 : 0);
                        case TokenType.Greater:      return Value.N(AsNum(L) >  AsNum(R) ? 1 : 0);
                        case TokenType.GreaterEqual: return Value.N(AsNum(L) >= AsNum(R) ? 1 : 0);
                        default: throw new Exception("Operator tidak dikenal");
                    }
                default:
                    throw new Exception("Expr tidak didukung");
            }
        }
    }

    // ===== PUBLIC API =====
    public void Run(string source)
    {
        // Peta keyword default + alias Dayak/Indonesia
        var keywordMap = new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase)
        {
            // Inggris standar
            ["let"] = TokenType.Let, ["print"] = TokenType.Print,
            ["if"]  = TokenType.If,  ["else"]  = TokenType.Else,
            ["true"] = TokenType.True, ["false"] = TokenType.False,

            // Alias lokal
            ["biar"]  = TokenType.Let,
            ["jituh"] = TokenType.Let,     // alias tambahan
            ["tulis"] = TokenType.Print,
            ["inulis"]= TokenType.Print,
            ["misal"] = TokenType.If,
            ["lain"]  = TokenType.Else,
            ["benar"] = TokenType.True,
            ["bujur"] = TokenType.True,
            ["salah"] = TokenType.False,
            ["sala"]  = TokenType.False,
        };

        var lexer = new Lexer(source, keywordMap);
        var tokens = lexer.Scan();
        var parser = new Parser(tokens);
        var program = parser.ParseProgram();
        var interp = new Interpreter(_output);
        interp.Exec(program);
    }
}
