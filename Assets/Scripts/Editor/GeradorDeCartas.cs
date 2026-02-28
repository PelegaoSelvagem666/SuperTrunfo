using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class GeradorDeCartas
{
    [MenuItem("Ferramentas/Gerar Deck de Fogo")]
    public static void GerarCartas()
    {
        string caminhoCSV = "Assets/Dados/deck_fogo.csv"; 
        string pastaDestinoAssets = "Assets/Cartas/DeckFogo/"; 
        
        // CORREÇÃO 1: O caminho exato da sua pasta de artes
        string pastaSprites = "Assets/Artes/ArtesDeckInicialFogo/"; 

        if (!AssetDatabase.IsValidFolder("Assets/Cartas")) AssetDatabase.CreateFolder("Assets", "Cartas");
        if (!AssetDatabase.IsValidFolder("Assets/Cartas/DeckFogo")) AssetDatabase.CreateFolder("Assets/Cartas", "DeckFogo");

        if (!File.Exists(caminhoCSV))
        {
            Debug.LogError($"Arquivo CSV não encontrado em: {caminhoCSV}");
            return;
        }

        string[] linhas = File.ReadAllLines(caminhoCSV);

        for (int i = 1; i < linhas.Length; i++)
        {
            string[] dados = linhas[i].Split(';'); 
            if (dados.Length < 10) continue; 

            CardData novaCarta = ScriptableObject.CreateInstance<CardData>();

            novaCarta.nomeCarta = dados[0];
            novaCarta.tituloHabilidade = dados[3];
            novaCarta.descricaoHabilidade = dados[4];
            novaCarta.forca = int.Parse(dados[5]);
            novaCarta.magia = int.Parse(dados[6]);
            novaCarta.agilidade = int.Parse(dados[7]);
            novaCarta.inteligencia = int.Parse(dados[8]);

            if (Enum.TryParse(dados[1], out CardType tipoParse)) novaCarta.tipo = tipoParse;
            if (Enum.TryParse(dados[2], out CardMoral moralParse)) novaCarta.moral = moralParse;

            // CORREÇÃO 2: Tenta procurar como PNG, se não achar, tenta JPG
            string nomeDoSprite = dados[9].Trim(); 
            string caminhoPNG = $"{pastaSprites}{nomeDoSprite}.png";
            string caminhoJPG = $"{pastaSprites}{nomeDoSprite}.jpg";
            
            Sprite spriteEncontrado = AssetDatabase.LoadAssetAtPath<Sprite>(caminhoPNG);
            if (spriteEncontrado == null) 
            {
                spriteEncontrado = AssetDatabase.LoadAssetAtPath<Sprite>(caminhoJPG);
            }

            if (spriteEncontrado != null)
            {
                novaCarta.arteCarta = spriteEncontrado;
            }
            else
            {
                Debug.LogWarning($"Sprite '{nomeDoSprite}' não encontrado! Verifique se o nome no CSV está idêntico ao arquivo na pasta.");
            }

            // CORREÇÃO 3: Força o cálculo da classe para não ficar tudo SS
            novaCarta.CalcularClasse();

            string nomeArquivoAsset = novaCarta.nomeCarta.Replace(" ", "_") + ".asset";
            AssetDatabase.CreateAsset(novaCarta, $"{pastaDestinoAssets}{nomeArquivoAsset}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Deck gerado com sucesso! Classes e Sprites atualizados.");
    }
}