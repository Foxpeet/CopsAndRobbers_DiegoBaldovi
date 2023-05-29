using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's
        for(int i=0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriu[i, j] = 0;
            }
        }
        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        // el contador indica el numero/indice de la casilla que estamos comprobando
        int contador = 0;
        for (int t = 0; t < Constants.NumTiles; t++)
        {
            
                //Casilla izquierda
                //comprobamos si la casilla que miramos no esta en el borde izquierdo
                if (contador % 8 != 0)
                {
                    matriu[contador, contador - 1] = 1;
                }
                //Casilla derecha
                //comprobamos si la casilla que miramos no esta en el borde derecho
                if ((contador + 1) % 8 != 0)
                {
                    matriu[contador, contador + 1] = 1;
                }
                //Casilla superior
                //Comprobamos que la casilla que miramos no esta en el borde superior
                if (contador > 7)
                {
                    matriu[contador, contador - 8] = 1;
                }
                //Casilla inferior
                //comprobamos que la casilla que miramos no esta en el borde inferior
                if (contador < 56)
                {
                    matriu[contador, contador + 8] = 1;
                }
            
            contador++;
        }
        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        
        //para cada casilla
        for(int m=0; m< Constants.NumTiles; m++)
        {
            List<int> adjacency = new List<int>();

            //si segun la matriz 'matriu' estan conectadas (hay un 1) se añade esa casilla a la lista adjacency correspondiente que se le dará a cada casilla
            for(int z=0; z< Constants.NumTiles; z++)
            {
                if (matriu[m, z] == 1)
                {
                    adjacency.Add(z);
                }
            }
            //la lista adjacency terminada de esa casilla, se la asignamos
            tiles[m].GetComponent<Tile>().adjacency = adjacency;
        }
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;

        //obtenemos la lista de casillas a las que puede ir por el BFS
        Tile[] tileList = FindSelectableTiles(false);

        //TODO: Cambia el código de abajo para hacer lo siguiente
        //- Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        int randomIndex = UnityEngine.Random.Range(0, tileList.Length);
        int tileToMove = tileList[randomIndex].numTile; //el indice de la casilla que ha salido aleatoriamente

        //- Actualizamos la variable currentTile del caco a la nueva casilla
        tiles[clickedTile].current = false;
        tiles[tileToMove].current = true;
        robber.GetComponent<RobberMove>().currentTile = tileToMove;

        //- Movemos al caco a esa casilla
        robber.GetComponent<RobberMove>().MoveToTile(tiles[robber.GetComponent<RobberMove>().currentTile]);
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public Tile[] FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;        

        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        /*for(int i = 0; i < Constants.NumTiles; i++)
        {
            tiles[i].selectable = true;
        }*/

        //añadimos los que estan a distancia 1 sin contar las casillas en las que haya un cop
        for(int z=0; z< tiles[indexcurrentTile].adjacency.Count; z++)
        {
            //si en esa casilla esta el cop 1 o el 2, no añadimos la casilla a la cola
            if(cops[0].GetComponent<CopMove>().currentTile != tiles[indexcurrentTile].adjacency[z] && cops[1].GetComponent<CopMove>().currentTile != tiles[indexcurrentTile].adjacency[z])
            {
                nodes.Enqueue(tiles[tiles[indexcurrentTile].adjacency[z]]);
                tiles[tiles[indexcurrentTile].adjacency[z]].selectable = true;
            }
        }
        //añadimos los que esten a distancia 1 de los obtenidos anteriormente (distancia 2 del origen) sin contar en las que haya un cop
        //creamos una cola auxiliar para no provocar un error en el foreach de la primera queue al añadir mas casillas
        Queue<Tile> newNodes = new Queue<Tile>();
        foreach (Tile tile in nodes)
        {
            //comprobamos para cada casilla de la primera queue todos sus adjacentes y los añadimos a la queue auxiliar
            for (int k = 0; k < tiles[tile.numTile].adjacency.Count; k++)
            {
                //si hay un cop en esa casilla no entramos al if y no la añadimos
                if (cops[0].GetComponent<CopMove>().currentTile != tiles[tile.numTile].adjacency[k] && cops[1].GetComponent<CopMove>().currentTile != tiles[tile.numTile].adjacency[k])
                {
                    //si la casilla adyacente que miramos no es la de origen
                    if (tiles[tile.numTile].adjacency[k] != indexcurrentTile) // & !nodes.Contains(tiles[k])
                    {
                        newNodes.Enqueue(tiles[tiles[tile.numTile].adjacency[k]]);
                        tiles[tiles[tile.numTile].adjacency[k]].selectable = true;
                    }

                }
            }
        }
        //un bucle para pasar todas las casillas de distancia 2 a la queue principal
        for (int i = 0; i < newNodes.Count; i++)
        {
            Tile tile = newNodes.Dequeue();
            nodes.Enqueue(tile);
        }
        //pasamos la queue a una lista para devolversela al robber y sea mas facil mirar la casilla aleatoria
        Tile[] List = new Tile[nodes.Count];
        nodes.CopyTo(List, 0);
        return List;
    }
    
}
