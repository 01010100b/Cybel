# Cybel

Cybel is a framework for General Game Playing and similar problems. A game is any class that implements the ```IGame``` interface. A player is any class that extends the ```Player``` base class. Games with the following properties are supported:
+ Any number of players.
+ No simultaneous moves.
+ Possibly non-deterministic. The method ```IGame.Perform(Move move)``` from a certain state is not required to transition to the same state each time, but could depend on a randomized factor (throwing dice, choosing a next tetromino piece, etc).
+ Must end in a finite number of turns.
+ Perfect information.

The ```Runner``` class can be used to play sets of games between players. There are 2 players included, a Matchbox player (see for example https://www.youtube.com/watch?v=R9c-_neaxeU) and an MCTS player. The only included game for now is the set of M,N,K games.