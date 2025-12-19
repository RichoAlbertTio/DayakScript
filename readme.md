# DYK: Bahasa Skrip Mini untuk Bahasa Dayak

DYK adalah bahasa skrip mini yang fun untuk memperkenalkan kosakata Bahasa Dayak lewat pemrograman.
Interpreter-nya ditulis dengan C# (.NET 8)

## Kosakata Bahasa Dayak

| Bahasa Dayak | Bahasa Indonesia | English | Fungsi |
|--------------|------------------|---------|---------|
| `jituh`      | biar/let        | let     | Deklarasi variabel |
| `inulis`     | tulis           | print   | Menampilkan output |
| `misal`      | jika            | if      | Kondisional |
| `beken`      | lainnya         | else    | Alternatif kondisional |
| `akan`       | untuk           | for     | Perulangan for |
| `katika`     | selama          | while   | Perulangan while |
| `gawi`       | kerjakan        | do      | Perulangan do-while |
| `benar`      | benar           | true    | Nilai boolean benar |
| `salah`      | salah           | false   | Nilai boolean salah |

## Fitur Singkat

* Insialiasi Variabel: `jituh`
* Assignment Variabel: `nama = nilai`
* Menampilkan teks: `inulis`
* Kondisional: `misal`…`beken` (alias `if`…`else`)
* Perulangan: `akan` (for), `katika` (while), `gawi`…`katika` (do-while)
* Angka & string (dengan escape `\"` `\\` `\n` `\t`)
* Operator: `+`, `-`, `*`, `/`, `==`, `!=`, `<`, `<=`, `>`, `>=`
* Komentar: `//` …

## Contoh

### Menampilkan nama
```dyk
// menampilkan nama
jituh nama = "Richo Albert Tio";
inulis "Halo nama saya, " + nama + "!";
```

### Percabangan misal / beken
```dyk
// inisialisasi variabel
jituh angka = 5;

// percabangan if-else
misal (angka > 5) {
  inulis "angka lebih besar dari 5";
} beken {
  inulis "angka kurang dari atau sama dengan 5";
}
```

### Perulangan
#### For Loop (akan)
```dyk
jituh i = 0;
akan (i < 5) {
    inulis "Iterasi: " + i;
    i = i + 1;
}
```

#### While Loop (katika)
```dyk
jituh j = 0;
katika (j < 3) {
    inulis "While: " + j;
    j = j + 1;
}
```

#### Do-While Loop (gawi...katika)
```dyk
jituh k = 0;
gawi {
    inulis "Do-while: " + k;
    k = k + 1;
} katika (k < 2);
```


### Cara Menjalankan
```bash
# CLI dayak (jika sudah di‐install sebagai .NET tool / exe)
dayak file.dyk

# Atau dengan dotnet run
dotnet run file.dyk
```


## Lisensi
MIT License
(c) Richo Albert Tio