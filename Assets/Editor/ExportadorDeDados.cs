using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ExportadorDeDados
{
    [MenuItem("Ferramentas/Gerar Relatorio do Deck")]
    public static void ExportarCartas()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== RELATÓRIO DO JOGO: CARTAS E HABILIDADES ===");
        sb.AppendLine();

        // Procura por todos os arquivos CardData no projeto
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData carta = AssetDatabase.LoadAssetAtPath<CardData>(path);
            
            if (carta != null)
            {
                string nomeHab = carta.habilidadeEspecial != null ? carta.habilidadeEspecial.name : "NENHUMA HABILIDADE";
                
                sb.AppendLine($"CARTA: {carta.nomeCarta}");
                sb.AppendLine($"  - Classificação: Classe {carta.classe} | Tipo: {carta.tipo} | Moral: {carta.moral}");
                sb.AppendLine($"  - Atributos: F:{carta.forca} | M:{carta.magia} | A:{carta.agilidade} | I:{carta.inteligencia}");
                sb.AppendLine($"  - Habilidade Equipada: {nomeHab}");
                sb.AppendLine("--------------------------------------------------");
            }
        }

        string caminho = Application.dataPath + "/RelatorioDoDeck.txt";
        File.WriteAllText(caminho, sb.ToString());
        
        Debug.Log("Relatório gerado com sucesso!");
        EditorUtility.RevealInFinder(caminho); // Abre a pasta do computador automaticamente
    }
}