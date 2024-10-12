using MyML;
using MyML.Functions;
using static System.Net.Mime.MediaTypeNames;

namespace LR2Form
{
    public partial class BaseForm : Form
    {
        private Random rnd = new Random(0);

        private double LearningRateStep { get; set; }
        public double LearningRate { get; private set; }
        public double EpochNumber { get; private set; }

        private NN nn { get; set; }
        private IActFunction func { get; set; }
        private string trainPath { get; set; }
        private string testPath { get; set; }

        private void UpdateLearningRateLabel()
        {
            LearningRateLabel.Text = $"�������� ��������: {LearningRate.ToString("F3")}";
        }
        private void UpdateEpochNumberLabel()
        {
            EpochNumberLabel.Text = $"����� ����: {EpochNumber}";
        }

        private List<Control> inputContols = [];

        private void DisableInput()
        {
            foreach (Control control in inputContols)
                control.Enabled = false;
        }
        private void EnableInput()
        {
            foreach (Control control in inputContols)
                control.Enabled = true;
        }

        private void CreateNN()
        {
            int[] nnparams = [64 * 64, 64, 32, 10];
            func = new Sigmoid();
            nn = new NN(nnparams, func);
        }

        public BaseForm()
        {
            CreateNN();

            trainPath = "training_data.csv";
            testPath = "validation_data.csv";

            InitializeComponent();
            LearningRateStep = 0.001;
            LearningRateTrackbar.Value = 20;
            EpochNumberTrackbar.Value = 4;

            LearningRate = LearningRateStep * LearningRateTrackbar.Value;
            EpochNumber = EpochNumberTrackbar.Value;
            UpdateLearningRateLabel();
            UpdateEpochNumberLabel();

            inputContols.Add(LearningRateTrackbar);
            inputContols.Add(EpochNumberTrackbar);
            inputContols.Add(LearnBTN);
            inputContols.Add(ShowstatsBTN);
            inputContols.Add(ShowgraphBTN);
        }

        private void LearningRateTrackbar_Scroll(object sender, EventArgs e)
        {
            LearningRate = LearningRateTrackbar.Value * LearningRateStep;
            UpdateLearningRateLabel();
        }

        private void EpochNumberTrackbar_Scroll(object sender, EventArgs e)
        {
            EpochNumber = EpochNumberTrackbar.Value;
            UpdateEpochNumberLabel();
        }

        private void LearnBTN_Click(object sender, EventArgs e)
        {
            DisableInput();
            Output.Text = "";

            Task.Run(() =>
            {
                string pathBase = DateTime.Now.ToString("yyyy.MM.dd_HH_mm_ss");
                int epochBase = nn.EpochCount;
                Output.Invoke(() =>
                {
                    Output.Text += $"������� ����: {epochBase} ����.\n";
                    Output.Text += $"������ ���������� �� {EpochNumber} ����\n";
                });

                for (int i = 1; i <= EpochNumber; i++)
                {
                    Output.Invoke(() =>
                    {
                        Output.Text += $"������ ����� �{epochBase + i}: {DateTime.Now.ToString("HH:mm:ss")}\n";
                    });
                    nn.EpochFromFiles(trainPath, testPath, LearningRate);
                    if (AutosaveCheckBox.Checked)
                    {
                        nn.ExportToJson($"{pathBase}_epoch_{epochBase + i}_autosave.json");
                        Output.Invoke(() =>
                        {
                            Output.Text += $"�������������� ������ �������\n";
                        });
                    }
                    var stats = nn.EpochStats[nn.EpochCount - 1];
                    Output.Invoke(() =>
                    {
                        Output.Text += $"������ �� ������������� �������: {stats.TrainingStats.Loss.ToString("F4")}\n";
                        Output.Text += $"������ �� ������������� �������: {stats.ValidationStats.Loss.ToString("F4")}\n";
                        Output.Text += $"�������� ���������\n";
                    });
                }
                Output.Invoke(() => EnableInput());
            });
        }

        private void ShowstatsBTN_Click(object sender, EventArgs e)
        {
            var estats = nn.EpochStats;
            string format = "F4";

            if (estats.Count < 1)
            {
                Output.Text = "��������� ��� �� �������. ������ ���";
                return;
            }

            Output.Text = $"��������� ������ {nn.EpochCount} ���� ��������\n";
            string nnparams = "";
            for (int i = 0; i < nn.LayerSizes.Length; i++)
            {
                nnparams += nn.LayerSizes[i].ToString();
                if (i < nn.LayerSizes.Length - 1)
                    nnparams += ":";
            }
            Output.Text += $"������� ����, ������� � ��������:\n{nnparams}\n\n";

            Output.Text += "������ �� ������������� �������:\n";
            for (int i = 0; i < nn.EpochCount; i++)
            {
                var stats = estats[i].TrainingStats;
                Output.Text += $"{stats.Loss.ToString(format)}; ";
            }
            Output.Text += "\n";
            Output.Text += "������ �� ������������� �������:\n";
            for (int i = 0; i < nn.EpochCount; i++)
            {
                var stats = estats[i].ValidationStats;
                Output.Text += $"{stats.Loss.ToString(format)}; ";
            }
            Output.Text += "\n\n";
            Output.Text += "ACCURACY:\n";
            for (int i = 0; i < nn.OutputSize; i++)
            {
                Output.Text += $"Accuracy ��� ������ {i} � �������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += $"{stats.Accuracy[i].ToString(format)}; ";
                }
                Output.Text += "\n";
                Output.Text += $"Accuracy ��� ������ {i} � ������������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += $"{stats.Accuracy[i].ToString(format)}; ";
                }
                Output.Text += "\n\n";
            }
            Output.Text += "\nPRECISION:\n";
            for (int i = 0; i < nn.OutputSize; i++)
            {
                Output.Text += $"Precision ��� ������ {i} � �������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += $"{stats.Precision[i].ToString(format)}; ";
                }
                Output.Text += "\n";
                Output.Text += $"Precision ��� ������ {i} � ������������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += $"{stats.Precision[i].ToString(format)}; ";
                }
                Output.Text += "\n\n";
            }
            Output.Text += "\nRECALL:\n";
            for (int i = 0; i < nn.OutputSize; i++)
            {
                Output.Text += $"Recall ��� ������ {i} � �������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += $"{stats.Recall[i].ToString(format)}; ";
                }
                Output.Text += "\n";
                Output.Text += $"Recall ��� ������ {i} � ������������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += $"{stats.Recall[i].ToString(format)}; ";
                }
                Output.Text += "\n\n";
            }
        }

        private void ShowgraphBTN_Click(object sender, EventArgs e)
        {
            var estats = nn.EpochStats;
            string format = "F4";

            if (estats.Count < 1)
            {
                Output.Text = "��������� ��� �� �������. ������ ���";
                return;
            }

            string Point(int x, double y)
            {
                return $"({x};{y.ToString(format)})";
            }

            Output.Text += "������ �� ������������� �������:\n";
            for (int i = 0; i < nn.EpochCount; i++)
            {
                var stats = estats[i].TrainingStats;
                Output.Text += Point(i + 1, stats.Loss);
            }
            Output.Text += "\n";
            Output.Text = "������ �� ������������� �������:\n";
            for (int i = 0; i < nn.EpochCount; i++)
            {
                var stats = estats[i].ValidationStats;
                Output.Text += Point(i + 1, stats.Loss);
            }
            Output.Text += "\n\n";
            Output.Text += "ACCURACY:\n";
            for (int i = 0; i < nn.OutputSize; i++)
            {
                Output.Text += $"Accuracy ��� ������ {i} � �������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += Point(j + 1, stats.Accuracy[i]);
                }
                Output.Text += "\n";
                Output.Text += $"Accuracy ��� ������ {i} � ������������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += Point(j + 1, stats.Accuracy[i]);
                }
                Output.Text += "\n\n";
            }
            Output.Text += "\nPRECISION:\n";
            for (int i = 0; i < nn.OutputSize; i++)
            {
                Output.Text += $"Precision ��� ������ {i} � �������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += Point(j + 1, stats.Accuracy[i]);
                }
                Output.Text += "\n";
                Output.Text += $"Precision ��� ������ {i} � ������������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += Point(j + 1, stats.Accuracy[i]);
                }
                Output.Text += "\n\n";
            }
            Output.Text += "\nRECALL:\n";
            for (int i = 0; i < nn.OutputSize; i++)
            {
                Output.Text += $"Recall ��� ������ {i} � �������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += Point(j + 1, stats.Accuracy[i]);
                }
                Output.Text += "\n";
                Output.Text += $"Recall ��� ������ {i} � ������������� �������:\n";
                for (int j = 0; j < nn.EpochCount; j++)
                {
                    var stats = estats[j].TrainingStats;
                    Output.Text += Point(j + 1, stats.Accuracy[i]);
                }
                Output.Text += "\n\n";
            }
        }

        private void SaveBTN_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                nn.ExportToJson(saveFileDialog.FileName);
            }
        }

        private void LoadBTN_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                nn = NN.BuildFromJson(openFileDialog.FileName, func);
                Output.Text = $"��������� ������ {nn.EpochCount} ���� ��������\n";
                string nnparams = "";
                for (int i = 0; i < nn.LayerSizes.Length; i++)
                {
                    nnparams += nn.LayerSizes[i].ToString();
                    if (i < nn.LayerSizes.Length - 1)
                        nnparams += ":";
                }
                Output.Text += $"������� ����, ������� � ��������:\n{nnparams}\n\n";
            }
        }

        private void ResetBTN_Click(object sender, EventArgs e)
        {
            CreateNN();
            Output.Text = "�������� ��������";
        }
    }
}