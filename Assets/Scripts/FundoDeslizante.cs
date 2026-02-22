using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class FundoDeslizante : MonoBehaviour
{
    [Header("Configuração de Direção e Velocidade")]
    public Vector2 velocidadeDeslizamento = new Vector2(0.015f, 0.005f); // Eixo X e Y
    
    private RawImage imagemFundo;

    void Start()
    {
        imagemFundo = GetComponent<RawImage>();
    }

    void Update()
    {
        // Desloca a textura internamente sem mover o objeto do UI
        Rect rect = imagemFundo.uvRect;
        rect.position += velocidadeDeslizamento * Time.deltaTime;
        imagemFundo.uvRect = rect;
    }
}