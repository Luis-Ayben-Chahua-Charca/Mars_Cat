// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Unity.Minigames;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MarsCat.EditorTools
{
    /// <summary>
    /// Tools → Mars Cat → Minijuegos → Fabrica de Suelo. Arma Canvas/Minijuegos/Minijuego_FabricaSuelo (granja de
    /// plantas, plant_farm) con SoilFactoryView, enlaza sus campos y lo registra en el MinigameHost.
    /// Idempotente: lo que ya existe no se toca. No guarda la escena.
    /// </summary>
    public static class SoilFactoryBuilder
    {
        private const float ColumnWidth = 460f;
        private const float BarHeight = 36f;
        private static readonly Color GuideMarkColor = new Color(1f, 0.9f, 0.2f, 1f);
        private static readonly Color ThresholdMarkColor = new Color(1f, 1f, 1f, 0.9f);

        [MenuItem("Tools/Mars Cat/Minijuegos/Fabrica de Suelo")]
        public static void Build()
        {
            MinigameUiKit.Begin("Crear minijuego Fabrica de Suelo");
            var (container, root) = MinigameUiKit.MinigameRoot("Minijuego_FabricaSuelo", "Fábrica de Suelo", new Vector2(1000f, 700f));

            // Columna 1: lavar
            RectTransform wash = Column(root, "ColumnaLavar", 20f);
            TMP_Text washHeader = Header(wash, "Encabezado", "1. Lavar el regolito");
            TMP_Text waterLabel = Label(wash, "EtiquetaAgua", -50f, "Agua de lavado");
            TMP_Text waterValue = Value(wash, "ValorAgua", -50f);
            (Slider waterSlider, RectTransform waterZone, RectTransform waterGuide) = SliderRow(wash, "Agua", -86f, 1f, 2f, 1f);
            TMP_Text waterGuideText = Small(wash, "GuiaAgua", -128f);
            TMP_Text perchlorateLabel = Label(wash, "EtiquetaPerclorato", -172f, "Perclorato restante");
            RectTransform perchlorateBar = MinigameUiKit.Node(wash, "MedidorPerclorato",
                MinigameUiKit.At(MinigameUiKit.TL, new Vector2(0f, -204f), new Vector2(ColumnWidth, 22f)));
            Image perchlorateFill = MinigameUiKit.Bar(perchlorateBar, new Color(0.3f, 0.9f, 0.4f, 1f));
            RectTransform thresholdMark = Mark(perchlorateBar, "MarcaUmbral", ThresholdMarkColor);
            TMP_Text perchlorateText = Small(wash, "TextoPerclorato", -232f);
            TMP_Text washStatus = Status(wash, "EstadoLavado", -270f);

            // Columna 2: mezclar
            RectTransform mix = Column(root, "ColumnaMezclar", 520f);
            TMP_Text mixHeader = Header(mix, "Encabezado", "2. Mezclar");
            TMP_Text compostLabel = Label(mix, "EtiquetaAbono", -50f, "Abono (% de la mezcla)");
            TMP_Text compostValue = Value(mix, "ValorAbono", -50f);
            (Slider compostSlider, RectTransform compostZone, RectTransform compostGuide) = SliderRow(mix, "Abono", -86f, 0f, 30f, 20f);
            TMP_Text compostGuideText = Small(mix, "GuiaAbono", -128f);
            TMP_Text compostStatus = Status(mix, "EstadoAbono", -156f, 40f);
            TMP_Text bacteriaLabel = Label(mix, "EtiquetaBacterias", -212f, "Bacterias (% de inóculo)");
            TMP_Text bacteriaValue = Value(mix, "ValorBacterias", -212f);
            (Slider bacteriaSlider, RectTransform bacteriaZone, RectTransform bacteriaGuide) = SliderRow(mix, "Bacterias", -248f, 1f, 10f, 5f);
            TMP_Text bacteriaGuideText = Small(mix, "GuiaBacterias", -290f);
            TMP_Text bacteriaStatus = Status(mix, "EstadoBacterias", -318f, 40f);

            // Resumen y botón
            RectTransform summary = MinigameUiKit.Node(root, "Resumen",
                r => MinigameUiKit.Set(r, MinigameUiKit.BL, MinigameUiKit.BR, MinigameUiKit.BC, new Vector2(0f, 50f), new Vector2(-40f, 190f)));
            TMP_Text yieldText = MinigameUiKit.Text(MinigameUiKit.Node(summary, "Rendimiento",
                MinigameUiKit.At(MinigameUiKit.TL, Vector2.zero, new Vector2(600f, 32f))),
                24f, FontStyles.Bold, TextAlignmentOptions.Left, "Rendimiento previsto");
            TMP_Text costText = MinigameUiKit.Text(MinigameUiKit.Node(summary, "Costo",
                MinigameUiKit.At(MinigameUiKit.TL, new Vector2(0f, -40f), new Vector2(600f, 56f))),
                18f, FontStyles.Normal, TextAlignmentOptions.TopLeft, "Costo", wrap: true);
            TMP_Text messageText = MinigameUiKit.Text(MinigameUiKit.Node(summary, "Mensaje",
                MinigameUiKit.At(MinigameUiKit.TL, new Vector2(0f, -104f), new Vector2(600f, 70f))),
                18f, FontStyles.Italic, TextAlignmentOptions.TopLeft, string.Empty, wrap: true);
            (Button confirmButton, TMP_Text confirmLabel) = MinigameUiKit.Button(
                MinigameUiKit.Node(summary, "BotonPreparar",
                    MinigameUiKit.At(MinigameUiKit.TR, new Vector2(0f, -40f), new Vector2(280f, 70f))),
                "Preparar");

            var view = MinigameUiKit.Add<SoilFactoryView>(container, out _);
            MinigameUiKit.Wire(view, so =>
            {
                MinigameUiKit.Ref(so, "_root", root.gameObject);
                MinigameUiKit.Ref(so, "_washHeader", washHeader);
                MinigameUiKit.Ref(so, "_waterLabel", waterLabel);
                MinigameUiKit.Ref(so, "_waterSlider", waterSlider);
                MinigameUiKit.Ref(so, "_waterValueText", waterValue);
                MinigameUiKit.Ref(so, "_waterZone", waterZone);
                MinigameUiKit.Ref(so, "_waterGuideMark", waterGuide);
                MinigameUiKit.Ref(so, "_waterGuideText", waterGuideText);
                MinigameUiKit.Ref(so, "_perchlorateLabel", perchlorateLabel);
                MinigameUiKit.Ref(so, "_perchlorateFill", perchlorateFill);
                MinigameUiKit.Ref(so, "_perchlorateThresholdMark", thresholdMark);
                MinigameUiKit.Ref(so, "_perchlorateText", perchlorateText);
                MinigameUiKit.Ref(so, "_washStatusText", washStatus);
                MinigameUiKit.Ref(so, "_mixHeader", mixHeader);
                MinigameUiKit.Ref(so, "_compostLabel", compostLabel);
                MinigameUiKit.Ref(so, "_compostSlider", compostSlider);
                MinigameUiKit.Ref(so, "_compostValueText", compostValue);
                MinigameUiKit.Ref(so, "_compostZone", compostZone);
                MinigameUiKit.Ref(so, "_compostGuideMark", compostGuide);
                MinigameUiKit.Ref(so, "_compostGuideText", compostGuideText);
                MinigameUiKit.Ref(so, "_compostStatusText", compostStatus);
                MinigameUiKit.Ref(so, "_bacteriaLabel", bacteriaLabel);
                MinigameUiKit.Ref(so, "_bacteriaSlider", bacteriaSlider);
                MinigameUiKit.Ref(so, "_bacteriaValueText", bacteriaValue);
                MinigameUiKit.Ref(so, "_bacteriaZone", bacteriaZone);
                MinigameUiKit.Ref(so, "_bacteriaGuideMark", bacteriaGuide);
                MinigameUiKit.Ref(so, "_bacteriaGuideText", bacteriaGuideText);
                MinigameUiKit.Ref(so, "_bacteriaStatusText", bacteriaStatus);
                MinigameUiKit.Ref(so, "_yieldText", yieldText);
                MinigameUiKit.Ref(so, "_costText", costText);
                MinigameUiKit.Ref(so, "_messageText", messageText);
                MinigameUiKit.Ref(so, "_confirmButton", confirmButton);
                MinigameUiKit.Ref(so, "_confirmLabel", confirmLabel);
            });
            MinigameUiKit.Register(view, "plant_farm");
            MinigameUiKit.End();
        }

        private static RectTransform Column(RectTransform root, string name, float x) =>
            MinigameUiKit.Node(root, name,
                r => MinigameUiKit.Set(r, MinigameUiKit.TL, MinigameUiKit.TL, MinigameUiKit.TL, new Vector2(x, -80f), new Vector2(ColumnWidth, 390f)));

        private static TMP_Text Header(RectTransform column, string name, string text) =>
            MinigameUiKit.Text(MinigameUiKit.Node(column, name,
                MinigameUiKit.At(MinigameUiKit.TL, Vector2.zero, new Vector2(ColumnWidth, 36f))),
                26f, FontStyles.Bold, TextAlignmentOptions.Left, text, color: MinigameUiKit.AccentColor);

        private static TMP_Text Label(RectTransform column, string name, float y, string text) =>
            MinigameUiKit.Text(MinigameUiKit.Node(column, name,
                MinigameUiKit.At(MinigameUiKit.TL, new Vector2(0f, y), new Vector2(ColumnWidth * 0.65f, 28f))),
                20f, FontStyles.Normal, TextAlignmentOptions.Left, text);

        private static TMP_Text Value(RectTransform column, string name, float y) =>
            MinigameUiKit.Text(MinigameUiKit.Node(column, name,
                MinigameUiKit.At(MinigameUiKit.TR, new Vector2(0f, y), new Vector2(ColumnWidth * 0.35f, 28f))),
                22f, FontStyles.Bold, TextAlignmentOptions.Right, "0");

        private static TMP_Text Small(RectTransform column, string name, float y) =>
            MinigameUiKit.Text(MinigameUiKit.Node(column, name,
                MinigameUiKit.At(MinigameUiKit.TL, new Vector2(0f, y), new Vector2(ColumnWidth, 26f))),
                16f, FontStyles.Normal, TextAlignmentOptions.Left, string.Empty);

        private static TMP_Text Status(RectTransform column, string name, float y, float height = 50f) =>
            MinigameUiKit.Text(MinigameUiKit.Node(column, name,
                MinigameUiKit.At(MinigameUiKit.TL, new Vector2(0f, y), new Vector2(ColumnWidth, height))),
                16f, FontStyles.Normal, TextAlignmentOptions.TopLeft, string.Empty, wrap: true);

        // Barra con slider, zona verde (rango aceptado) y marca amarilla (valor guía). La vista coloca zona y marca.
        private static (Slider slider, RectTransform zone, RectTransform guide) SliderRow(
            RectTransform column, string name, float y, float min, float max, float guide)
        {
            RectTransform bar = MinigameUiKit.Node(column, "Barra" + name,
                MinigameUiKit.At(MinigameUiKit.TL, new Vector2(0f, y), new Vector2(ColumnWidth, BarHeight)));
            Slider slider = MinigameUiKit.Slider(MinigameUiKit.Node(bar, "Slider", MinigameUiKit.Stretch), min, max, min);
            // Mismos márgenes que la zona de la manija, para que las marcas coincidan con el recorrido del slider.
            RectTransform marks = MinigameUiKit.Node(bar, "Marcas", r => MinigameUiKit.Margins(r, 10f, 10f, 0f, 0f));
            RectTransform zone = MinigameUiKit.Node(marks, "ZonaRango", MinigameUiKit.Stretch);
            MinigameUiKit.Panel(zone, new Color(0.2f, 0.85f, 0.35f, 0.3f));
            RectTransform guideMark = Mark(marks, "MarcaGuia", GuideMarkColor);
            return (slider, zone, guideMark);
        }

        // Marca vertical fina; la vista fija sus anclas en X.
        private static RectTransform Mark(RectTransform parent, string name, Color color)
        {
            RectTransform mark = MinigameUiKit.Node(parent, name,
                r => MinigameUiKit.Set(r, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), MinigameUiKit.C, Vector2.zero, new Vector2(4f, 0f)));
            MinigameUiKit.Panel(mark, color);
            return mark;
        }
    }
}
