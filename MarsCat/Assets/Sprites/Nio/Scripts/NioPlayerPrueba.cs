using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

/// <summary>
/// Prueba automática de Nio_Player en Play Mode (Tools > Nio > 6). Se añade solo durante la prueba.
/// Simula la entrada de NioMovement (las mismas rutas de código que el teclado) y mide el resultado real:
/// activación, movimiento, animaciones, cámara, colisiones e integración visual. Escribe Logs/NioPlayer_PruebaPlayMode.txt.
/// </summary>
[DisallowMultipleComponent]
public class NioPlayerPrueba : MonoBehaviour
{
    NioMovement mov;
    CharacterController cc;
    NioAnimacion anim;
    Animator animator;
    SpriteRenderer sr;
    Camera cam;
    Quaternion rotCamInicial;
    float maxDesvioRotCam;
    readonly StringBuilder sb = new StringBuilder();
    readonly StringBuilder notas = new StringBuilder();
    int ok, total;
    Texture2D hoja;
    RenderTexture rt;
    int capturas;
    const int CapW = 480, CapH = 270, CapCols = 3, CapFilas = 2;

    // Registro continuo
    float sueloMin = float.MaxValue, sueloMax = float.MinValue, spriteBajoSuelo = float.MaxValue;
    int framesSuelo;
    SpriteRenderer silSr;
    Sprite spritePrevio;
    float signoPrevio = 1f;
    int framesSil, silOk;

    static string Ruta(string n)
    {
        string d = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs");
        Directory.CreateDirectory(d);
        return Path.Combine(d, n);
    }

    void C(bool cond, string texto)
    {
        total++;
        if (cond) ok++;
        sb.AppendLine((cond ? "[OK]    " : "[FALLO] ") + texto);
    }

    string Estado()
    {
        AnimatorStateInfo i = animator.GetCurrentAnimatorStateInfo(0);
        foreach (string e in new[] { "Idle", "Walk", "Work", "Back" }) if (i.IsName(e)) return e;
        return "?";
    }

    Vector3 Adelante => Vector3.ProjectOnPlane(mov.movementReference.forward, Vector3.up).normalized;
    Vector3 Derecha => Vector3.ProjectOnPlane(mov.movementReference.right, Vector3.up).normalized;
    Vector3 CamAdelante => Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
    Vector3 CamDerecha => Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
    static Vector3 Plano(Vector3 v) { v.y = 0f; return v; }

    Vector2 EntradaHacia(Vector3 dirMundo)
    {
        dirMundo = Plano(dirMundo).normalized;
        return new Vector2(Vector3.Dot(dirMundo, Derecha), Vector3.Dot(dirMundo, Adelante));
    }

    IEnumerator Esperar(float s)
    {
        float t = 0f;
        while (t < s) { yield return null; t += Time.deltaTime; }
    }

    void LateUpdate()
    {
        if (cam == null || mov == null) return;
        maxDesvioRotCam = Mathf.Max(maxDesvioRotCam, Quaternion.Angle(rotCamInicial, cam.transform.rotation));
        if (mov.HaySuelo && cc.isGrounded)
        {
            float gap = mov.visual.position.y - mov.AlturaSuelo;
            sueloMin = Mathf.Min(sueloMin, gap);
            sueloMax = Mathf.Max(sueloMax, gap);
            spriteBajoSuelo = Mathf.Min(spriteBajoSuelo, sr.bounds.min.y - mov.AlturaSuelo);
            framesSuelo++;
        }
        if (silSr != null)
        {
            framesSil++;
            float sx = silSr.transform.localScale.x;
            bool spriteOk = silSr.sprite == sr.sprite || silSr.sprite == spritePrevio;
            float signo = sr.flipX ? -1f : 1f;
            bool flipOk = Mathf.Approximately(sx, signo) || Mathf.Approximately(sx, signoPrevio);
            if (spriteOk && flipOk && silSr.enabled) silOk++;
            spritePrevio = sr.sprite;
            signoPrevio = signo;
        }
    }

    IEnumerator Start()
    {
        mov = GetComponent<NioMovement>();
        cc = GetComponent<CharacterController>();
        anim = mov != null ? mov.animacion : null;
        animator = anim != null ? anim.GetComponent<Animator>() : null;
        sr = anim != null ? anim.GetComponent<SpriteRenderer>() : null;
        cam = Camera.main;
        Transform silT = mov != null && mov.visual != null ? mov.visual.Find("Silueta") : null;
        silSr = silT != null ? silT.GetComponent<SpriteRenderer>() : null;
        sb.AppendLine("PRUEBA DE NIO_PLAYER EN PLAY MODE — " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | Unity " + Application.unityVersion);
        if (mov == null || anim == null || animator == null || sr == null || cam == null || mov.movementReference == null)
        {
            sb.AppendLine("ERROR: faltan referencias (NioMovement, Visual, Animator, cámara o MovementReference).");
            Terminar();
            yield break;
        }
        hoja = new Texture2D(CapW * CapCols, CapH * CapFilas, TextureFormat.RGB24, false);
        rt = new RenderTexture(CapW, CapH, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

        yield return Esperar(1.0f); // aterrizar y estabilizar la cámara
        rotCamInicial = cam.transform.rotation;
        maxDesvioRotCam = 0f;
        Vector3 inicio = transform.position;

        // ---------------- PRUEBA 1: activación
        sb.AppendLine();
        sb.AppendLine("== PRUEBA 1: activación (tecla C) ==");
        C(!mov.controlActivo, "Al iniciar la escena el control está desactivado");
        mov.usarEntradaSimulada = true;
        mov.entradaSimulada = new Vector2(0f, 1f);
        yield return Esperar(0.6f);
        float deriva = Plano(transform.position - inicio).magnitude;
        C(deriva < 0.005f, "Con el control desactivado, mantener W no mueve a Nio (desplazamiento " + deriva.ToString("0.0000") + " m)");
        mov.entradaSimulada = Vector2.zero;
        mov.PulsarAlternarControl();
        yield return Esperar(0.1f);
        C(mov.controlActivo, "C activa el control");
        mov.PulsarTrabajar();
        yield return Esperar(0.3f);
        C(Estado() == "Work", "E en reposo pone a Nio a trabajar (estado " + Estado() + ")");
        mov.PulsarAlternarControl();
        yield return Esperar(0.4f);
        C(!mov.controlActivo && Estado() == "Work", "C desactiva el control sin perder la animación (estado " + Estado() + ")");
        mov.PulsarAlternarControl();
        yield return Esperar(0.3f);
        C(mov.controlActivo && Estado() == "Work", "C lo reactiva y Nio sigue en Work");
        mov.PulsarTrabajar();
        yield return Esperar(0.3f);
        C(Estado() == "Idle", "E de nuevo vuelve a Idle");

        // ---------------- PRUEBAS 2 y 3: movimiento y animaciones
        sb.AppendLine();
        sb.AppendLine("== PRUEBAS 2 y 3: movimiento y animaciones ==");
        Vector3 zona = BuscarZonaLibre(inicio);
        mov.Teletransportar(zona);
        yield return Esperar(1.5f);
        notas.AppendLine("Zona libre usada para las pruebas de movimiento: " + zona.ToString("0.00"));
        yield return ProbarTecladoReal(zona);
        var casos = new (string nombre, Vector2 entrada, bool correr)[]
        {
            ("W", new Vector2(0, 1), false), ("S", new Vector2(0, -1), false), ("A", new Vector2(-1, 0), false), ("D", new Vector2(1, 0), false),
            ("W+D", new Vector2(1, 1), false), ("S+A", new Vector2(-1, -1), false), ("Shift+D", new Vector2(1, 0), true)
        };
        foreach (var caso in casos)
        {
            mov.Teletransportar(zona);
            yield return Esperar(1.2f);
            yield return ProbarMovimiento(caso.nombre, caso.entrada, caso.correr);
        }
        CapturaCamara();

        // ---------------- PRUEBA 4: cámara
        sb.AppendLine();
        sb.AppendLine("== PRUEBA 4: cámara ==");
        yield return ProbarCamara(zona);

        // ---------------- PRUEBA 5: colisiones
        sb.AppendLine();
        sb.AppendLine("== PRUEBA 5: colisiones ==");
        yield return ProbarMuro(zona);
        yield return ProbarMobiliario();
        yield return ProbarPuerta();
        yield return ProbarRecorrido();

        // ---------------- PRUEBA 6: integración visual
        sb.AppendLine();
        sb.AppendLine("== PRUEBA 6: integración visual ==");
        C(framesSuelo > 100 && sueloMin >= -0.001f && sueloMax <= 0.012f,
          "Pies sobre el suelo en " + framesSuelo + " frames: separación pivote-suelo entre " + (sueloMin * 1000f).ToString("0.0") + " y " + (sueloMax * 1000f).ToString("0.0") + " mm");
        C(spriteBajoSuelo >= -0.001f, "El sprite nunca se mete bajo el suelo (borde inferior mínimo " + (spriteBajoSuelo * 1000f).ToString("0.0") + " mm sobre el suelo)");
        Transform sombra = mov.sombra;
        var srs = sombra != null ? sombra.GetComponent<SpriteRenderer>() : null;
        C(srs != null && srs.sortingOrder < sr.sortingOrder && Mathf.Abs(sombra.position.y - mov.AlturaSuelo - mov.elevacionSombra) < 0.003f,
          "Sombra circular bajo los pies, dibujada antes que Nio (orden " + (srs != null ? srs.sortingOrder : 0) + " < " + sr.sortingOrder + ")");
        notas.AppendLine("CharacterController: separación cápsula-suelo en reposo " + ((transform.position.y - mov.AlturaSuelo) * 1000f).ToString("0.0") + " mm (el sprite se apoya en el suelo con el ajuste de pies)");
        float altoNio = AltoIdle();
        notas.AppendLine("Tamaño: Nio mide " + altoNio.ToString("0.00") + " m en Idle = " + (altoNio / 2.43f * 100f).ToString("0") + " % de una puerta de 2,43 m y " +
                         (altoNio / 1.38f * 100f).ToString("0") + " % de una consola de 1,38 m");
        C(silSr != null && framesSil > 100 && silOk == framesSil,
          "Silueta a través de muros activa y sincronizada con el sprite de Nio en " + silOk + "/" + framesSil + " frames");
        C(maxDesvioRotCam < 0.5f, "La cámara mantiene la inclinación isométrica durante toda la prueba (desvío máximo " + maxDesvioRotCam.ToString("0.00") + "°)");

        Terminar();
    }

    float AltoIdle()
    {
        return sr.sprite != null ? sr.sprite.rect.height / sr.sprite.pixelsPerUnit * sr.transform.lossyScale.y : 0f;
    }

    IEnumerator ProbarTecladoReal(Vector3 zona)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) { notas.AppendLine("Teclado real: no hay dispositivo Keyboard."); yield break; }
        mov.usarEntradaSimulada = false;
        bool antes = mov.controlActivo;
        yield return Tecla(kb, 0.1f, Key.C);
        bool cambio = mov.controlActivo != antes;
        if (!cambio)
        {
            notas.AppendLine("Los eventos de teclado inyectados en el Input System no llegaron al juego (en el editor el teclado solo llega con la vista Game enfocada).");
            mov.usarEntradaSimulada = true;
            yield break;
        }
        yield return Tecla(kb, 0.1f, Key.C);
        C(cambio && mov.controlActivo == antes, "Teclado real (eventos del Input System): C desactiva y vuelve a activar el control");
        foreach (var (tecla, dir) in new[] { (Key.W, Adelante), (Key.D, Derecha) })
        {
            mov.Teletransportar(zona);
            yield return Esperar(0.5f);
            Vector3 p0 = transform.position;
            yield return Tecla(kb, 0.5f, tecla);
            yield return Esperar(0.3f);
            Vector3 d = Plano(transform.position - p0);
            C(d.magnitude > 0.8f && Vector3.Angle(d, dir) < 3f,
              "Teclado real: " + tecla + " mueve a Nio " + d.magnitude.ToString("0.00") + " m hacia " + dir.ToString("0.00") + " (desvío " + Vector3.Angle(d, dir).ToString("0.0") + "°)");
        }
        mov.Teletransportar(zona);
        mov.usarEntradaSimulada = true;
        yield return Esperar(0.8f);
#else
        yield break;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    IEnumerator Tecla(Keyboard kb, float duracion, Key k)
    {
        InputSystem.QueueStateEvent(kb, new KeyboardState(k));
        yield return Esperar(duracion);
        InputSystem.QueueStateEvent(kb, new KeyboardState());
        yield return null;
        yield return null;
    }
#endif

    IEnumerator ProbarMovimiento(string nombre, Vector2 entrada, bool correr)
    {
        Vector2 e = entrada.sqrMagnitude > 1f ? entrada.normalized : entrada;
        Vector3 esperado = (Derecha * e.x + Adelante * e.y).normalized;
        float vEsperada = correr ? mov.velocidadCorrer : mov.velocidad;
        bool alejarse = mov.usarBackAlAlejarse && Vector3.Dot(esperado, CamAdelante) >= mov.umbralAlejarse;
        string estadoEsperado = alejarse ? "Back" : "Walk";
        float lateral = Vector3.Dot(esperado, CamDerecha);

        mov.entradaSimulada = entrada;
        mov.correrSimulado = correr;
        yield return Esperar(0.3f);
        Vector3 p1 = transform.position;
        float t1 = Time.time;
        int frames = 0, enEstado = 0, flipOk = 0;
        float yMin = float.MaxValue, yMax = float.MinValue;
        var otros = new HashSet<string>();
        float t = 0f;
        while (t < 0.6f)
        {
            yield return null;
            t += Time.deltaTime;
            frames++;
            string est = Estado();
            if (est == estadoEsperado) enEstado++; else otros.Add(est);
            bool flipEsperado = lateral < 0f;
            if (Mathf.Abs(lateral) < 0.1f || sr.flipX == flipEsperado) flipOk++;
            yMin = Mathf.Min(yMin, transform.position.y);
            yMax = Mathf.Max(yMax, transform.position.y);
        }
        Vector3 d = Plano(transform.position - p1);
        float v = d.magnitude / Mathf.Max(0.0001f, Time.time - t1);
        float ang = Vector3.Angle(d, esperado);
        Vector3 pantalla = cam.WorldToViewportPoint(transform.position) - cam.WorldToViewportPoint(p1);
        float angPantalla = Mathf.Atan2(pantalla.x, pantalla.y) * Mathf.Rad2Deg;

        mov.entradaSimulada = Vector2.zero;
        mov.correrSimulado = false;
        float tParada = 0f;
        while (Estado() != "Idle" && tParada < 1f) { yield return null; tParada += Time.deltaTime; }
        Vector3 pParada = transform.position;
        yield return Esperar(0.3f);
        float derivaParada = Plano(transform.position - pParada).magnitude;

        C(ang < 3f && Mathf.Abs(v - vEsperada) / vEsperada < 0.08f && yMax - yMin < 0.01f,
          nombre + ": se mueve hacia " + esperado.ToString("0.00") + " del escenario (desvío " + ang.ToString("0.0") + "°) a " + v.ToString("0.00") + " m/s (esperado " +
          vEsperada.ToString("0.00") + "), altura estable (±" + ((yMax - yMin) * 1000f).ToString("0.0") + " mm); en pantalla avanza a " + angPantalla.ToString("0") + "° de la vertical");
        C(enEstado == frames && flipOk == frames,
          nombre + ": animación " + estadoEsperado + " en " + enEstado + "/" + frames + " frames" + (otros.Count > 0 ? " (otros: " + string.Join(",", otros) + ")" : "") +
          (Mathf.Abs(lateral) >= 0.1f ? "; mira a la " + (lateral < 0f ? "izquierda (flipX)" : "derecha") : ""));
        C(Estado() == "Idle" && tParada < 0.4f && derivaParada < 0.002f,
          nombre + ": al soltar vuelve a Idle en " + tParada.ToString("0.00") + " s y queda quieto (deriva " + (derivaParada * 1000f).ToString("0.0") + " mm)");
    }

    IEnumerator ProbarCamara(Vector3 zona)
    {
        mov.Teletransportar(zona);
        yield return Esperar(1.5f);
        Vector3 dir = BuscarDireccionLibre(zona, 2.5f);
        Vector2 entrada = EntradaHacia(dir);
        float maxSalto = 0f, retrasoMax = 0f;
        Vector3 camPrev = cam.transform.position;
        Vector3 offsetInicial = cam.transform.position - mov.cameraTarget.position;
        mov.entradaSimulada = entrada;
        float t = 0f;
        while (t < 1.0f)
        {
            yield return null;
            t += Time.deltaTime;
            maxSalto = Mathf.Max(maxSalto, (cam.transform.position - camPrev).magnitude);
            camPrev = cam.transform.position;
            retrasoMax = Mathf.Max(retrasoMax, ((cam.transform.position - mov.cameraTarget.position) - offsetInicial).magnitude);
        }
        mov.entradaSimulada = Vector2.zero;
        yield return Esperar(1.8f);
        Vector3 camAntes = cam.transform.position;
        yield return Esperar(0.2f);
        float movCamFinal = (cam.transform.position - camAntes).magnitude;
        Vector3 vp = cam.WorldToViewportPoint(mov.cameraTarget.position);
        float dist = Vector3.Distance(cam.transform.position, mov.cameraTarget.position);
        // Posición diseñada: centro horizontal y un 10 % por debajo del centro (viewport y = 0.4); zona muerta ±3 %.
        float desvioEncuadre = Mathf.Max(Mathf.Abs(vp.x - 0.5f), Mathf.Abs(vp.y - 0.4f));
        C(desvioEncuadre <= 0.035f && movCamFinal < 0.005f,
          "Seguimiento: tras recorrer " + (mov.velocidad * 1f).ToString("0.0") + " m y parar, la cámara vuelve a encuadrar a Nio y se detiene (a " +
          (desvioEncuadre * 100f).ToString("0.0") + " % de la posición prevista, dentro de la zona muerta del 3 %; movimiento de la cámara en el último 0,2 s: " +
          (movCamFinal * 1000f).ToString("0.0") + " mm)");
        C(retrasoMax > 0.05f && maxSalto < 0.2f, "Suavizado: la cámara se queda atrás hasta " + retrasoMax.ToString("0.00") + " m y se mueve como máximo " + maxSalto.ToString("0.000") + " m por frame (sin saltos)");
        C(vp.x > 0.4f && vp.x < 0.6f && vp.y > 0.32f && vp.y < 0.5f,
          "Encuadre: Nio queda en el centro, un poco por debajo (posición en pantalla x=" + vp.x.ToString("0.00") + ", y=" + vp.y.ToString("0.00") + ")");
        Vector3 eu = cam.transform.eulerAngles;
        C(!cam.orthographic && Mathf.Abs(dist - 10f) < 1.0f,
          "Perspectiva isométrica: inclinación " + eu.x.ToString("0.0") + "°, orientación " + eu.y.ToString("0.0") + "°, FOV " + cam.fieldOfView.ToString("0") + "°, distancia " + dist.ToString("0.0") + " m");
        CapturaCamara();
    }

    IEnumerator ProbarMuro(Vector3 zona)
    {
        mov.Teletransportar(zona);
        yield return Esperar(0.5f);
        RaycastHit mejor = default;
        bool hay = false;
        for (int i = 0; i < 16; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, i * 22.5f, 0f) * Vector3.forward;
            foreach (RaycastHit h in Physics.RaycastAll(zona + Vector3.up * 0.3f, dir, 8f, ~0, QueryTriggerInteraction.Ignore).OrderBy(q => q.distance))
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                bool recto = h.collider is BoxCollider;
                bool mejorRecto = hay && mejor.collider is BoxCollider;
                if (h.collider.bounds.size.y >= 2f && (!hay || (recto && !mejorRecto) || (recto == mejorRecto && h.distance < mejor.distance))) { mejor = h; hay = true; }
                break;
            }
        }
        if (!hay) { C(false, "Muro: no se encontró un muro cercano para la prueba"); yield break; }
        Vector3 haciaMuro = Plano(mejor.point - zona).normalized;
        mov.entradaSimulada = EntradaHacia(haciaMuro);
        yield return Esperar(mejor.distance / mov.velocidad + 1.5f);
        mov.entradaSimulada = Vector2.zero;
        yield return Esperar(0.3f);
        Vector3 fin = transform.position;
        bool cruzo = Physics.Linecast(zona + Vector3.up * 0.3f, fin + Vector3.up * 0.3f, out RaycastHit hc, ~0, QueryTriggerInteraction.Ignore) && hc.collider == mejor.collider;
        float distMuro = Vector3.Dot(mejor.point - fin, -mejor.normal);
        Vector3 cc0 = fin + Vector3.up * (cc.radius + 0.06f), cc1 = fin + Vector3.up * (cc.height - cc.radius);
        bool solapa = Physics.OverlapCapsule(cc0, cc1, cc.radius - 0.03f, ~0, QueryTriggerInteraction.Ignore).Any(x => x == mejor.collider);
        C(!cruzo && !solapa,
          "Muro " + mejor.collider.name + ": empujando contra él durante " + (mejor.distance / mov.velocidad + 1.5f).ToString("0.0") + " s, Nio se queda a " + distMuro.ToString("0.00") +
          " m de la superficie (radio de la cápsula " + cc.radius + " m), sin atravesarlo ni solaparse");
        // deslizamiento a lo largo del muro (diagonal contra él)
        Vector3 tangente = Vector3.Cross(Vector3.up, mejor.normal).normalized;
        Vector3 antes = transform.position;
        mov.entradaSimulada = EntradaHacia((haciaMuro + tangente).normalized);
        yield return Esperar(0.6f);
        mov.entradaSimulada = Vector2.zero;
        yield return Esperar(0.2f);
        float desliz = Mathf.Abs(Vector3.Dot(transform.position - antes, tangente));
        C(desliz > 0.3f, "Junto al muro, empujando en diagonal Nio se desliza a lo largo de él sin quedarse atascado (" + desliz.ToString("0.00") + " m en 0,6 s)");
    }

    IEnumerator ProbarMobiliario()
    {
        BoxCollider mueble = null;
        Vector3 inicio = Vector3.zero;
        cc.enabled = false;
        foreach (BoxCollider b in Todos<BoxCollider>().Where(x => !x.isTrigger && x.name.StartsWith("COL_Obs_") && x.bounds.min.y < 0.05f)
                     .OrderBy(x => Vector3.Distance(x.bounds.center, transform.position)))
        {
            Vector3 c0 = Plano(b.bounds.center);
            for (int i = 0; i < 16 && mueble == null; i++)
                foreach (float dist in new[] { 1.0f, 0.8f, 1.4f })
                {
                    Vector3 dir = Quaternion.Euler(0f, i * 22.5f, 0f) * Vector3.forward;
                    Vector3 borde = Plano(b.ClosestPoint(b.bounds.center + dir * 5f));
                    Vector3 p = borde + dir * dist;
                    if (!Libre(p)) continue;
                    if (Physics.SphereCast(p + Vector3.up * 0.3f, cc.radius, (c0 - p).normalized, out RaycastHit h, 4f, ~0, QueryTriggerInteraction.Ignore) && h.collider == b)
                    {
                        mueble = b;
                        inicio = p;
                        break;
                    }
                }
            if (mueble != null) break;
        }
        cc.enabled = true;
        if (mueble == null) { C(false, "Mobiliario: no se encontró un acceso libre a ninguna consola o terminal"); yield break; }
        Vector3 c = Plano(mueble.bounds.center);
        mov.Teletransportar(inicio);
        yield return Esperar(0.6f);
        mov.entradaSimulada = EntradaHacia(c - inicio);
        yield return Esperar(2.0f);
        mov.entradaSimulada = Vector2.zero;
        yield return Esperar(0.2f);
        Vector3 cp = mueble.ClosestPoint(transform.position + Vector3.up * 0.3f);
        float d = Plano(cp - transform.position).magnitude;
        C(d >= cc.radius - 0.03f, "Mobiliario " + mueble.name + " (" + mueble.bounds.size.y.ToString("0.00") + " m de alto): caminando 2 s contra él, Nio se detiene a " +
                                   d.ToString("0.00") + " m (radio " + cc.radius + "), no lo atraviesa");
    }

    IEnumerator ProbarPuerta()
    {
        CabinaPuertaAutomatica puerta = Todos<CabinaPuertaAutomatica>().FirstOrDefault(p => p.puertaId == "TEC_Norte")
                                        ?? Todos<CabinaPuertaAutomatica>().FirstOrDefault(p => p.puertaId == "TEC_Sur");
        if (puerta == null || puerta.colliderBloqueo == null) { C(false, "Puerta: no se encontró TEC_Norte/TEC_Sur"); yield break; }
        Bounds b = puerta.colliderBloqueo.bounds;
        Vector3 normal = b.size.x < b.size.z ? Vector3.right : Vector3.forward;
        Vector3 centro = Plano(b.center);
        Vector3 lado = Mathf.Sign(Vector3.Dot(-centro, normal)) * normal; // lado hacia el centro de la cabina
        Vector3 inicio = centro + lado * 1.4f;
        Vector3 destino = centro - lado * 0.9f;
        if (!Libre(inicio)) { C(false, "Puerta " + puerta.puertaId + ": el punto de inicio no está libre"); yield break; }

        bool estabaBloqueada = puerta.bloqueada;
        puerta.bloqueada = true;
        mov.Teletransportar(inicio);
        yield return Esperar(1.0f);
        mov.entradaSimulada = EntradaHacia(-lado);
        yield return Esperar(2.5f);
        mov.entradaSimulada = Vector2.zero;
        float lado1 = Vector3.Dot(Plano(transform.position) - centro, lado);
        C(lado1 >= cc.radius - 0.03f && puerta.Apertura01 < 0.01f,
          "Puerta " + puerta.puertaId + " cerrada con llave: Nio se detiene delante (" + lado1.ToString("0.00") + " m del plano de la puerta) y no la atraviesa");

        puerta.bloqueada = false;
        int aperturas = puerta.Aperturas;
        mov.entradaSimulada = EntradaHacia(destino - transform.position);
        float t = 0f;
        while (t < 5f && Vector3.Dot(Plano(transform.position) - centro, lado) > -0.5f) { yield return null; t += Time.deltaTime; }
        mov.entradaSimulada = Vector2.zero;
        float lado2 = Vector3.Dot(Plano(transform.position) - centro, lado);
        C(lado2 <= -0.5f && puerta.Aperturas > aperturas,
          "Puerta " + puerta.puertaId + " normal: se abre sola al acercarse Nio y la cruza en " + t.ToString("0.0") + " s");
        mov.entradaSimulada = EntradaHacia(lado);
        yield return Esperar(2.0f);
        mov.entradaSimulada = Vector2.zero;
        puerta.bloqueada = estabaBloqueada;
        yield return Esperar(0.5f);
    }

    IEnumerator ProbarRecorrido()
    {
        var puntos = new List<(string tramo, Vector3 p)> { ("inicio", CabinaDatos.InicioRecorrido) };
        puntos.AddRange(CabinaDatos.Recorrido.Select(x => (x.tramo, x.posicion)));
        mov.Teletransportar(puntos[0].p);
        yield return Esperar(1.0f);
        int alcanzados = 0, muestras = 0, visibles = 0;
        bool capturaTapado = false;
        float yMin = float.MaxValue;
        string fallo = null;
        float tTotal = 0f, distancia = 0f;
        Vector3 prev = transform.position;
        int[] marcasCaptura = { puntos.Count / 5, 2 * puntos.Count / 5, 3 * puntos.Count / 5, 4 * puntos.Count / 5 };
        for (int i = 1; i < puntos.Count && fallo == null; i++)
        {
            Vector3 objetivo = puntos[i].p;
            float limite = Plano(objetivo - transform.position).magnitude / mov.velocidadCorrer * 2.5f + 3f;
            float t = 0f, sinAvance = 0f, mejorDist = float.MaxValue;
            mov.correrSimulado = true;
            while (Plano(objetivo - transform.position).magnitude > 0.3f)
            {
                mov.entradaSimulada = EntradaHacia(objetivo - transform.position);
                yield return null;
                t += Time.deltaTime;
                tTotal += Time.deltaTime;
                distancia += Plano(transform.position - prev).magnitude;
                prev = transform.position;
                yMin = Mathf.Min(yMin, transform.position.y);
                float dAct = Plano(objetivo - transform.position).magnitude;
                if (dAct < mejorDist - 0.01f) { mejorDist = dAct; sinAvance = 0f; } else sinAvance += Time.deltaTime;
                muestras++;
                Vector3 o = cam.transform.position;
                if (Visible(o, transform.position + Vector3.up * 0.32f) || Visible(o, transform.position + Vector3.up * 0.58f)) visibles++;
                else if (!capturaTapado && t > 0.5f) { capturaTapado = true; CapturaCamara(); notas.AppendLine("Captura con Nio tapado por un muro (se ve su silueta): tramo '" + puntos[i].tramo + "'"); }
                if (transform.position.y < -0.5f) { fallo = "Nio cayó por debajo del suelo en el tramo '" + puntos[i].tramo + "'"; break; }
                if (sinAvance > 3f || t > limite) { fallo = "Nio se quedó bloqueado en el tramo '" + puntos[i].tramo + "' cerca de " + transform.position.ToString("0.00"); break; }
            }
            if (fallo == null) alcanzados++;
            if (marcasCaptura.Contains(i)) CapturaCamara();
        }
        mov.entradaSimulada = Vector2.zero;
        mov.correrSimulado = false;
        yield return Esperar(0.5f);
        C(fallo == null, "Recorrido completo de la auditoría (" + (puntos.Count - 1) + " tramos por núcleo, 6 módulos, pasillos y accesos): " + alcanzados + "/" + (puntos.Count - 1) +
                         " puntos alcanzados, " + distancia.ToString("0.0") + " m en " + tTotal.ToString("0.0") + " s" + (fallo != null ? " — " + fallo : ""));
        C(yMin > -0.02f, "Nunca cae a través del escenario (altura mínima " + yMin.ToString("0.000") + " m)");
        float pct = 100f * visibles / Mathf.Max(1, muestras);
        notas.AppendLine("Visibilidad desde la cámara durante el recorrido (sin muros entre la cámara y el centro o la cabeza de Nio): " + pct.ToString("0.0") + " % de " + muestras + " frames");
        notas.AppendLine("Con la inclinación original los muros de 3 m tapan a Nio en el " + (100f - pct).ToString("0.0") + " % de los frames; en esos casos se dibuja su silueta.");
    }

    IEnumerable<T> Todos<T>() where T : Component => gameObject.scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(false));

    bool Visible(Vector3 o, Vector3 p)
    {
        Vector3 d = p - o;
        foreach (RaycastHit h in Physics.RaycastAll(o, d.normalized, d.magnitude - 0.03f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.transform.IsChildOf(transform)) continue;
            if (h.collider.bounds.min.y > 2.5f) continue;
            return false;
        }
        return true;
    }

    bool Libre(Vector3 p)
    {
        Vector3 a = p + Vector3.up * (cc.radius + 0.05f), b = p + Vector3.up * (cc.height - cc.radius + 0.02f);
        return !Physics.OverlapCapsule(a, b, cc.radius + 0.03f, ~0, QueryTriggerInteraction.Ignore).Any(x => !x.transform.IsChildOf(transform));
    }

    Vector3 BuscarDireccionLibre(Vector3 p, float largo)
    {
        cc.enabled = false;
        Vector3 r = Derecha;
        foreach (Vector3 d in new[] { Derecha, -Derecha, Adelante, -Adelante })
            if (!Physics.SphereCast(p + Vector3.up * 0.3f, cc.radius, d, out _, largo, ~0, QueryTriggerInteraction.Ignore)) { r = d; break; }
        cc.enabled = true;
        return r;
    }

    Vector3 BuscarZonaLibre(Vector3 cerca)
    {
        Vector3 mejor = cerca;
        float mejorD = float.MaxValue;
        Vector3[] dirs = { Adelante, -Adelante, Derecha, -Derecha, (Adelante + Derecha).normalized, (-Adelante - Derecha).normalized };
        cc.enabled = false;
        for (float x = -12f; x <= 12f; x += 0.5f)
            for (float z = -14f; z <= 14f; z += 0.5f)
            {
                Vector3 p = new Vector3(x, 0f, z);
                if (!Physics.Raycast(p + Vector3.up * 2.5f, Vector3.down, out RaycastHit h, 3f, ~0, QueryTriggerInteraction.Ignore) || h.point.y > 0.05f) continue;
                p.y = h.point.y;
                if (!Libre(p)) continue;
                bool bien = true;
                foreach (Vector3 d in dirs)
                    if (Physics.SphereCast(p + Vector3.up * 0.3f, cc.radius + 0.02f, d, out _, 2.3f, ~0, QueryTriggerInteraction.Ignore)) { bien = false; break; }
                if (!bien) continue;
                float dist = Vector3.Distance(p, cerca);
                if (dist < mejorD) { mejorD = dist; mejor = p; }
            }
        cc.enabled = true;
        return mejor;
    }

    void CapturaCamara()
    {
        if (capturas >= CapCols * CapFilas) return;
        try
        {
            var req = new RenderPipeline.StandardRequest { destination = rt };
            if (RenderPipeline.SupportsRenderRequest(cam, req)) RenderPipeline.SubmitRenderRequest(cam, req);
            else { RenderTexture prevT = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = prevT; }
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            int col = capturas % CapCols, fila = capturas / CapCols;
            hoja.ReadPixels(new Rect(0, 0, CapW, CapH), col * CapW, (CapFilas - 1 - fila) * CapH);
            RenderTexture.active = prev;
            capturas++;
        }
        catch (System.Exception e) { notas.AppendLine("Captura no disponible: " + e.Message); }
    }

    void Terminar()
    {
        if (mov != null) { mov.usarEntradaSimulada = false; mov.entradaSimulada = Vector2.zero; mov.correrSimulado = false; }
        sb.AppendLine();
        sb.AppendLine("== NOTAS ==");
        sb.Append(notas);
        sb.AppendLine();
        sb.AppendLine("RESULTADO: " + ok + "/" + total + " comprobaciones superadas");
        File.WriteAllText(Ruta("NioPlayer_PruebaPlayMode.txt"), sb.ToString());
        if (hoja != null && capturas > 0)
        {
            hoja.Apply();
            File.WriteAllBytes(Ruta("NioPlayer_PruebaPlayMode_camara.png"), hoja.EncodeToPNG());
        }
        if (ok == total) Debug.Log("[Nio] Prueba de Nio_Player: " + ok + "/" + total + " OK. Informe: Logs/NioPlayer_PruebaPlayMode.txt");
        else Debug.LogWarning("[Nio] Prueba de Nio_Player: " + ok + "/" + total + " OK. Revisa Logs/NioPlayer_PruebaPlayMode.txt");
        if (rt != null) rt.Release();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Destroy(this);
    }
}
