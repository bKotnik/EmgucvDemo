#EmguCV naloga

-Pod File->Open se zbere poljubno sliko
-Pod Utility->Binarize se sliko binarizira (zbere se tudi threshold)
	*tukaj se zgodi, da pride do napake z indeksi, če pride do tega, je treba zbrati drugi threshold
-Pod Process->Find holes se pa poišče luknje (ubistvu se poišče konture)
-Project/solution lahko vrne error v zvezi z EmguCV, ko se prvič odpre. Odpravi se s tem, da se enostavno
 ali "clean solution" ali pa "unload project" in potem spet "load project"

*rezultati niso čisto točni