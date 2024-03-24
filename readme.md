My project to dots challange to make a gol simulation in unity with dots https://itch.io/jam/dots-challenge-1
My optimalizations:
  - one entity == one chunk of cells (8 by 8) stored in single ulong
  - no storage of future/past chunk version (only borders)
  - custom render (sadly witchout custom shaders - would be much faster)
  - calculate all cells in chunk at once using bit operations (practically no if statements in simulation logic or forloop for each cell)
  - option to slow/skip frame rendering (render every 4 frame etc)

This is image of how compare 4 inputs and get if 0/1/3/4 are active, then you easy get if 2. Then you can group neighbors into 2 groups and only compare numbers of neoghbors between each other
![logic gates](https://github.com/andruplay9/gameOfLifeForDotsChallange032024/assets/57016413/4e5fc60c-752a-45d1-8829-8ea2843d65b4)

60 frames without skipping give 59x59 chunks (its 472x472 - 200k)
![image](https://github.com/andruplay9/gameOfLifeForDotsChallange032024/assets/57016413/dfc60482-75eb-4639-a111-c9df218e6175)

30 frames witch skipping every 8 frames get 130x 130 chunks (it 1040x1040 - 1081600)
![image](https://github.com/andruplay9/gameOfLifeForDotsChallange032024/assets/57016413/4b92ca9a-a738-43d7-9f4d-9f74045eb672)

P.S App use Vcontainer + MessagePipe for comunication between ui-ecs, not affect simulation in any way

