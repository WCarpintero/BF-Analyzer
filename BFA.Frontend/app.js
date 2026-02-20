const { createApp, ref, computed } = Vue;

createApp({
    setup() {
        // Estado reactivo alineado con el nuevo formulario
        const form = ref({
            idea: '',
            targetName: '',
            offeringType: 'Producto', // Valor por defecto
            location: '',
            industry: '',
            budget: 0
        });

        const loading = ref(false);
        const result = ref(null);
        const score = ref(0);
        const trendText = ref('N/A');
        let chart = null;

        // Propiedad computada para cambiar el color del score dinámicamente
        const scoreColor = computed(() => {
            if (score.value >= 70) return 'text-emerald-400';
            if (score.value >= 40) return 'text-orange-400';
            return 'text-red-500';
        });

        const analyze = async () => {
            // Validación extendida
            if (!form.value.idea || !form.value.targetName || !form.value.location) {
                alert("Por favor completa los campos principales (Idea, Producto y Ubicación).");
                return;
            }

            loading.value = true;
            result.value = null;
            score.value = 0; // Reiniciamos para la animación

            try {
                const response = await fetch('http://localhost:5072/api/feasibility/analyze', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        businessIdea: form.value.idea,
                        targetName: form.value.targetName,
                        offeringType: form.value.offeringType,
                        targetLocation: form.value.location,
                        industry: form.value.industry,
                        budget: parseFloat(form.value.budget) || 0
                    })
                });

                const data = await response.json();
                const rawResponse = data.result || "";

                // --- LÓGICA DE PARSING (Extracción de datos de la IA) ---
                
                // 1. Extraer Score 
                const scoreMatch = rawResponse.match(/SCORE:\s*(\d+)/i);
                if (scoreMatch) {
                    // Animación simple: sube de 0 al valor en 1 segundo
                    setTimeout(() => {
                        score.value = Math.min(parseInt(scoreMatch[1]), 100);
                    }, 100);
                }

                // 2. Extraer Trend (Busca "TREND: ALTA")
                const trendMatch = rawResponse.match(/TREND:\s*(\w+)/i);
                if (trendMatch) trendText.value = trendMatch[1].toUpperCase();

                // 3. Extraer Report (Busca todo lo después de "REPORT:")
                const reportMatch = rawResponse.match(/REPORT:\s*([\s\S]*)/i);
                result.value = reportMatch ? reportMatch[1].trim() : rawResponse;

                // 4. Renderizar gráfico
                renderChart();

            } catch (error) {
                console.error("Error:", error);
                alert("Error crítico en la conexión con BF Analyzer.");
            } finally {
                loading.value = false;
            }
        };

        const renderChart = () => {
            setTimeout(() => {
                const canvas = document.getElementById('trendsChart');
                if (!canvas) return;
                const ctx = canvas.getContext('2d');
                if (chart) chart.destroy();

                // Configuración de colores dinámica
                const isHigh = trendText.value.includes('ALTA');
                const colorPrimary = isHigh ? '#10b981' : '#3b82f6'; // Verde si es alta, azul si no.
                const colorFill = isHigh ? 'rgba(16, 185, 129, 0.1)' : 'rgba(59, 130, 246, 0.1)';

                chart = new Chart(ctx, {
                    type: 'line',
                    data: {
                        // Meses actualizados a febrero 2026
                        labels: ['Sep 25', 'Oct 25', 'Nov 25', 'Dic 25', 'Ene 26', 'Feb 26'], 
                        datasets: [{
                            label: 'Interés de Mercado',
                            data: isHigh ? [40, 55, 60, 85, 90, 100] : [20, 30, 25, 40, 35, 50],
                            borderColor: colorPrimary,
                            backgroundColor: colorFill,
                            borderWidth: 4,
                            fill: true,
                            tension: 0.4,
                            pointRadius: 5,
                            pointBackgroundColor: colorPrimary,
                            pointBorderColor: '#060a11',
                            pointBorderWidth: 2
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: { 
                            legend: { display: false },
                            tooltip: {
                                backgroundColor: '#0f172a',
                                titleFont: { family: 'JetBrains Mono' },
                                bodyFont: { family: 'JetBrains Mono' },
                                displayColors: false
                            }
                        },
                        scales: {
                            y: { 
                                beginAtZero: true,
                                max: 100,
                                grid: { color: 'rgba(255, 255, 255, 0.03)' },
                                ticks: { color: '#475569', font: { size: 10 } }
                            },
                            x: { 
                                grid: { display: false },
                                ticks: { color: '#475569', font: { size: 10 } }
                            }
                        }
                    }
                });
            }, 1000);
        };

        return { form, loading, result, score, trendText, scoreColor, analyze };
    }
}).mount('#app');