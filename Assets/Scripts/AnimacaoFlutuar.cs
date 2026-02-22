using UnityEngine;

public class AnimacaoFlutuar : MonoBehaviour
{
    [Header("Configurações de Flutuação")]
    public float amplitudeRotacao = 1.5f; 
    public float velocidade = 1.2f;
    
    private float desvioAleatorio;
    private Quaternion rotacaoInicial;
    private bool estavaNaMao; // Rastreador para saber quando a carta mudou de lugar

    void Start()
    {
        rotacaoInicial = transform.localRotation;
        desvioAleatorio = Random.Range(0f, 100f); 
        
        if (GameManager.instancia != null)
        {
            estavaNaMao = (transform.parent == GameManager.instancia.maoJogador || transform.parent == GameManager.instancia.maoAdversario);
        }
    }

    void Update()
    {
        if (GameManager.instancia == null) return;

        // Verifica fisicamente onde a carta está neste exato frame
        bool naMao = (transform.parent == GameManager.instancia.maoJogador || transform.parent == GameManager.instancia.maoAdversario);

        if (naMao)
        {
            // Se está na mão, flutua suavemente
            float anguloZ = Mathf.Sin((Time.time + desvioAleatorio) * velocidade) * amplitudeRotacao;
            transform.localRotation = rotacaoInicial * Quaternion.Euler(0f, 0f, anguloZ);
            estavaNaMao = true;
        }
        else if (estavaNaMao)
        {
            // Se ela acabou de sair da mão (foi arrastada pra mesa), endireita a carta imediatamente!
            // Fazemos isso apenas UMA VEZ para não bloquear o efeito de pilha bagunçada do cemitério depois.
            transform.localRotation = rotacaoInicial;
            estavaNaMao = false;
        }
    }
}