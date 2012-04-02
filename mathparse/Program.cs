using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Math expression parser based off shunting-yard algorithm, written by alex 4/1/2012
//TODO -- implement bitwise operations? (&, |, <<, >>, etc) (maybe repurpose ** to exponent)
namespace mathparse
{
    public class Token
    {   }
    public class NumToken : Token
    {
        public double value;
        public int length; //used for determining how much to advance in the string
        public NumToken(double val, int l)
        {
            value = val;
            length = l;
        }
    }
    public class OpToken : Token
    {
        public OpType type;        
        public int precedence; //precendence of the operator (PEMDAS)
        public bool leftasso; //is the operator left associative?
        public OpToken(OpType type)
        {
            this.type = type;
            switch (type)
            {
                case OpType.ADD:
                    precedence = 0;
                    leftasso = true;
                    break;
                case OpType.SUB:
                    precedence = 0;
                    leftasso = true;
                    break;
                case OpType.MUL:
                    precedence = 1;
                    leftasso = true;
                    break;
                case OpType.DIV:
                    precedence = 1;
                    leftasso = true;
                    break;
                case OpType.MOD:
                    precedence = 1;
                    leftasso = true;
                    break;
                case OpType.LP:
                    precedence = -1; //these will be handled specially
                    break;
                case OpType.RP:
                    precedence = -1; //these wil be handled specially
                    break;
                case OpType.EXP:
                    precedence = 2;
                    leftasso = false;
                    break;
            }
        }
    }
    public enum OpType
    {
        ADD, //addition '+'
        SUB, //subtraction '-'
        MUL, //multiplication '*'
        DIV, //division '/'
        LP, //left parentheses '('
        RP, //right parentheses ')'
        EXP, //exponent '^'
        MOD, //modulo '%'
        NONE, 
    }
    public class Program
    {     
        static void Main(string[] args)
        {
            Console.WriteLine("Input an expression. Note that _ is the negative sign.");
            string exp = Console.ReadLine();            
            exp = exp.Replace(" ", ""); //strip spaces
            Stack<NumToken> nums = new Stack<NumToken>(); //number stack
            Stack<OpToken> ops = new Stack<OpToken>(); //operator stack            
            for (int i = 0; i < exp.Length; ) //increments of i will be determined based off whether it is a number or an operator
            {
                Token k = null;
                try
                {
                    k = getNextToken(exp, i);
                }
                catch (Exception )
                {
                    Console.WriteLine("Error parsing token.");
                    break;
                }
                if (k is NumToken)
                {
                    NumToken nt = (NumToken)k;
                    nums.Push(nt);
                    i += nt.length;
                }
                else if (k is OpToken)
                {
                    OpToken x = (OpToken)k;
                    OpToken y = ops.Count > 0 ? ops.Peek() : null;
                    if (y == null || x.type == OpType.LP) //if stack has nothing in it yet, or if the token is a (, push it on stack, no evaluation
                    {
                        ops.Push(x);
                    }
                    else
                    {
                        if (x.type == OpType.RP) //if the current token is a ), unwind the stack until a ( is encountered
                        {
                            OpToken alpha = null; 
                            while ((alpha = ops.Pop()).type != OpType.LP)
                            {
                                applyOperator(alpha, ref nums);
                            }
                        }
                        else
                        {
                            //apply operator if the current token is <= in precedence than the on on the top of the stack (< if right associative)
                            while ((x.leftasso && x.precedence <= y.precedence) || (!x.leftasso && x.precedence < y.precedence))
                            {
                                ops.Pop(); //pop y off, we're applying it
                                applyOperator(y, ref nums);
                                if (ops.Count == 0)
                                {
                                    break;
                                }
                                y = ops.Peek(); //look at the next operator on stack in case it can be applied as well
                            }
                            ops.Push(x);
                        }
                    }
                    i++;
                }
            }
            //unwind remaining operators
            while (ops.Count > 0)
            {
                OpToken y = ops.Pop();
                applyOperator(y, ref nums);
            }
            if (nums.Count > 0)
            {
                Console.WriteLine(exp + " = " + nums.Pop().value);
            }
            else
            {
                Console.WriteLine("No result.");
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(false);
        }

        static void applyOperator(OpToken t, ref Stack<NumToken> nums)
        {
            if (t.type == OpType.LP || t.type == OpType.RP)
            {
                Console.WriteLine("Parenthesis mixmatch");
                return;
            }
            try
            {
                double op2 = nums.Pop().value;
                double op1 = nums.Pop().value;
                switch (t.type)
                {
                    case OpType.ADD:
                        nums.Push(new NumToken(op1 + op2, -1));
                        break;
                    case OpType.SUB:
                        nums.Push(new NumToken(op1 - op2, -1));
                        break;
                    case OpType.MUL:
                        nums.Push(new NumToken(op1 * op2, -1));
                        break;
                    case OpType.DIV:
                        nums.Push(new NumToken(op1 / op2, -1));
                        break;
                    case OpType.EXP:
                        nums.Push(new NumToken(Math.Pow(op1, op2), -1));
                        break;
                    case OpType.MOD:
                        nums.Push(new NumToken(op1 % op2, -1));
                        break;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Encountered exception, please check your syntax");
            }
        }
        static Token getNextToken(string s, int p)
        {
            OpType guess = OpType.NONE;
            switch (s[p])
            {
                case '+':
                    guess = OpType.ADD;
                    break;
                case '-':
                    guess = OpType.SUB;
                    break;
                case '*':
                    guess = OpType.MUL;
                    break;
                case '/':
                    guess = OpType.DIV;
                    break;
                case '(':
                    guess = OpType.LP;
                    break;
                case ')':
                    guess = OpType.RP;
                    break;
                case '^':
                    guess = OpType.EXP;
                    break;
                case '%':
                    guess = OpType.MOD;
                    break;
            }
            if (guess != OpType.NONE)
            {
                return new OpToken(guess);
            }
            string num = "";
            for (int i = p; i < s.Length; i++)
            {
                if (char.IsDigit(s[i]) || s[i] == '.' || s[i] == '_')
                {
                    num += s[i];
                }
                else
                {
                    break;
                }
            }
            num = num.Replace('_', '-'); //_ is the negative sign for sheer ease
            if (num != "")
            {
                return new NumToken(Double.Parse(num), num.Length);

            }
            throw new Exception("could not parse token");
        }
    }
}
