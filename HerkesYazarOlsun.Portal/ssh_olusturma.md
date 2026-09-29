from docx import Document
from docx.shared import Pt

doc=Document()
doc.add_heading('GitHub: HTTPS ve SSH Kullanımı (macOS)',1)
p=doc.add_paragraph()
p.add_run('HTTPS:\n').bold=True
p.add_run('- Kurulumu kolaydır.\n- PAT veya Credential Manager/Keychain ile çalışır.\n- İlk girişten sonra genellikle tekrar kimlik bilgisi istemez.\n\n')
p.add_run('SSH:\n').bold=True
p.add_run('- SSH anahtarı ile kimlik doğrulama yapar.\n- Kullanıcı adı/PAT istemez.\n- Çoklu cihaz ve profesyonel kullanım için yaygındır.\n\n')
doc.add_heading('SSH Anahtarı Oluşturma',2)
for cmd in [
"ssh-keygen -t ed25519 -C \"mail@example.com\"",
"eval \"$(ssh-agent -s)\"",
"ssh-add --apple-use-keychain ~/.ssh/id_ed25519",
"cat ~/.ssh/id_ed25519.pub",
"ssh -T git@github.com"
]:
    para=doc.add_paragraph(style='List Bullet')
    para.add_run(cmd)
doc.add_heading('Remote Türünü Kontrol Etme',2)
doc.add_paragraph("git remote -v")
doc.add_heading('HTTPS -> SSH Dönüştürme',2)
doc.add_paragraph("git remote set-url origin git@github.com:mansur61/HerkesYazarOlsun.UI.git")
doc.add_heading('Ne Zaman SSH Kullanmalıyım?',2)
doc.add_paragraph("- HTTPS sorunsuz çalışıyorsa geçmek zorunlu değildir.\n- Birden fazla bilgisayar veya hesap kullanıyorsan SSH önerilir.\n- Tek hesap ve sorunsuz HTTPS kullanıyorsan mevcut yapı yeterlidir.")
path="/mnt/data/GitHub_HTTPS_vs_SSH_macOS.docx"
doc.save(path)
print(path)
