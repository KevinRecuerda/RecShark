grammar ExpressionEvaluator;
 
@parser::header 
{
    #pragma warning disable 3021
}
@parser::members
{
    protected const int EOF = Eof;
}

@lexer::header 
{
    #pragma warning disable 3021
}
@lexer::members
{
    protected const int EOF = Eof;
    protected const int HIDDEN = Hidden;
}
 

/*
 * Parser Rules
 */

safeExpr 
    : expr EOF
    ;

expr
	: expr POW expr                                     # PowerExpr
	| expr SQUARE                                       # SquareExpr
	| MINUS expr                                        # ChangeSignExpr
	| NOT expr                                          # NotExpr
	| expr op=(MULT | DIV | MOD) expr                   # MultOrDivOrModExpr
	| expr op=(PLUS | MINUS) expr                       # PlusOrMinusExpr
	| expr op=(LE | GE | LT | GT) expr                  # RelationalExpr
	| expr op=(MATCH | NOMATCH) expr                    # MatchingExpr
	| expr op=(EQ | NE) expr                            # EqualityExpr
	| expr op=(IN | NOTIN) params                       # InExpr
	| expr AND expr                                     # AndExpr
	| expr OR expr                                      # OrExpr
	|<assoc=right> expr QUESTION expr COLON expr        # TernaryExpr
	| func                                              # ToFuncExpr
	| atom                                              # ToAtomExpr
	;

func
	: funcName params   # Function
	;

params
	: OBRACE (expr COMMA)* expr CBRACE   # Parameters
	;

atom 
	: OBRACE expr CBRACE   # Braces
	| num                  # Number
	| bool                 # Boolean
	| const                # Constant
	| var                  # Variable
	| str                  # String
	;
	 
funcName
	: var
	;

var 
	: ID
	;

const 
	: PI	# ConstPi
	; 
	
num 
	: NUMBER
	;

str
	: STRING
	;

bool
	: TRUE
	| FALSE
	;



/*
 * Lexer Rules
 */

// Constants
PI : 'pi' | 'PI' | 'Pi';

// Operators
PLUS    : '+';
MINUS   : '-';
MULT    : '*';
DIV     : '/';
POW     : '^';
MOD     : '%';
SQUARE  : '²';

// Conditional Operators
AND : '&&';
OR  : '||';

// Equality Operators
EQ : '==';
NE : '!=';

// Matching Operators
MATCH : '=~';
NOMATCH : '!=~';

// Unary Operators
NOT  : '!';

// Relational Operators
GT : '>';
GE : '>=';
LT : '<';
LE : '<=';

// Other Operators
IN    : 'in'     | 'IN'     | 'In';
NOTIN : 'not in' | 'NOT IN' | 'Not In';

// Booleans
TRUE  : 'true'  | 'TRUE'  | 'True';
FALSE : 'false' | 'FALSE' | 'False';

// Basis
OBRACE   : '(';
CBRACE   : ')';
COMMA    : ',';
QUESTION : '?';
COLON    : ':';

ID
	: [a-zA-Z] [a-zA-Z_0-9]*
	;

NUMBER
	: [0-9]+ ('.' [0-9]+)?
	;

STRING
	: '"' (~["\r\n] | '""')* '"'
	;

WS : (' ' | '\t' | '\r' | '\n') -> channel(HIDDEN);