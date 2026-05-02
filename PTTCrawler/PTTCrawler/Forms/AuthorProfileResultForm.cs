using System.Text;
using PTTCrawler.Models;

namespace PTTCrawler.Forms
{
    public partial class AuthorProfileResultForm : Form
    {
        private readonly AuthorProfile _profile;

        public AuthorProfileResultForm(AuthorProfile profile)
        {
            InitializeComponent();
            _profile = profile;
            LoadProfile();
        }

        private void LoadProfile()
        {
            lblGenderValue.Text       = _profile.Gender.DisplayText;
            lblAgeValue.Text          = _profile.Age.DisplayText;
            lblAreaValue.Text         = _profile.ResidentialArea.DisplayText;
            lblInterestsValue.Text    = _profile.Interests.DisplayText;
            lblRelationshipValue.Text = _profile.RelationshipStatus.DisplayText;
            lblOccupationValue.Text   = _profile.Occupation.DisplayText;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"性別：{_profile.Gender.DisplayText}");
            sb.AppendLine($"年紀：{_profile.Age.DisplayText}");
            sb.AppendLine($"居住地區：{_profile.ResidentialArea.DisplayText}");
            sb.AppendLine($"興趣：{_profile.Interests.DisplayText}");
            sb.AppendLine($"感情狀態：{_profile.RelationshipStatus.DisplayText}");
            sb.AppendLine($"職業/身份：{_profile.Occupation.DisplayText}");
            Clipboard.SetText(sb.ToString());
            MessageBox.Show("已複製到剪貼簿。", "複製", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
