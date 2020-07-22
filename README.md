# EmguCV naloga

-Pod File->Open se zbere poljubno sliko <br />
-Pod Utility->Binarize se sliko binarizira (zbere se tudi threshold) <br />
	*tukaj se zgodi, da pride do napake z indeksi, če pride do tega, je treba zbrati drugi threshold <br />
-Pod Process->Find holes se pa poišče luknje (ubistvu se poišče konture) <br />
-Project/solution lahko vrne error v zvezi z EmguCV, ko se prvič odpre. Odpravi se s tem, da se enostavno <br />
 ali "clean solution" ali pa "unload project" in potem spet "load project" <br />
<br />
*rezultati niso čisto točni <br />