Italics: *italics* _italics_
bold: **bold** __bold__
Header: #header1, ##header2, ###header3,.... ######header6
	Header with line beneath:
		#header1
		======
		
		#header2
		------
Horizantal line: 3 or more ---, ***, ___
Paragraph:
	Spaces are preserved?
	2 spaces at the end of a line signal start a new line
list:
	Unordered : can use *, -, +
	* Item 1
	* Item 2
	  * Item 2a
	  * Item 2b
	Ordered: What the number is doesn't mean anything.  Automatically just counts up.
	1. Item 1
	1. Item 2
	1. Item 3
	   1. Item 3a
	   1. Item 3b
Images: ![text](url)
Link: 	[text](url)
	Link with title: [text](url "mouse hover title")
	Referance style link: [text][case-sensitive referance text]
	Referance to repository file: [text](../blob/master/LICENSE)
Blockquotes:
	>words
	>words

GITHUB FLAVORED MARKDOWN
	CrossOut: ~~word~~
	Highlighting:
		```
			highlight box around this text
		```
		```javascript
			code, syntax highlighting
		```
		or indent code by 4 spaces
	Inline highlighting
		not `is highlited` not
	Table
		at least 3 dashes seperating each header cell
		outline pipes (|) are optional
		
		| left aligned  | centered   | right aligned  |
		| ------------- |:-------------:| -----:|
		| col 3 is      | right-aligned | $1600 |
		| col 2 is      | centered      |   $12 |
		| zebra stripes | are neat      |    $1 |
		
	Can use raw HTML
		<dl>
		  <dt>Definition list</dt>
		  <dd>Is something people use sometimes.</dd>

		  <dt>Markdown in HTML</dt>
		  <dd>Does *not* work **very** well. Use HTML <em>tags</em>.</dd>
		</dl>
