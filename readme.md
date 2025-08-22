# DYK: Bahasa Skrip Mini untuk Bahasa Dayak

DYK adalah bahasa skrip mini yang fun untuk memperkenalkan kosakata Bahasa Dayak lewat pemrograman.
Interpreter-nya ditulis dengan C# (.NET 8)

## Fitur Singkat

* Insialiasi Variabel: `jituh`
* Menampilkan teks: `inulis`
* Kondisional: `misal`…`beken` (alias `if`…`else`)
* Angka & string (dengan escape `\"` `\\` `\n` `\t`)
* Operator: `+`, `-`, `*`, `/`, `==`, `!=`, `<`, `<=`, `>`, `>=`
* Komentar: `//` …

## Contoh

### Menampilkan nama

### Menampilkan nama
// menampilkan nama
jituh nama = "Richo Albert Tio";
inulis "Halo nama saya, " + nama + "!";

### Percabangan misal / beken
// inisialisasi variabel
jituh angka = 5;

// percabangan if-else
misal (angka > 5) {
  inulis "angka lebih besar dari 5";
} beken {
  inulis "angka kurang dari atau sama dengan 5";
}


### Cara Menjalankan
CLI dayak (jika sudah di‐install sebagai .NET tool / exe)
dayak file.dyk
